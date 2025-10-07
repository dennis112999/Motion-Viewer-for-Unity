#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Dennis.Tools.MotionViewer
{
    /// <summary>
    /// Motion Preview Window
    /// </summary>
    public class MotionPreviewWindow : EditorWindow
    {
        // Preview: Model & AnimationClip
        private PreviewRenderUtility _previewRenderUtility;
        private GameObject _previewGO;
        private Animator _animator;
        private AnimatorController _animatorController;
        private AnimatorState _previewState;
        private MotionData _motionData;

        // Preview Settings
        private Vector2 _previewSize = new Vector2(640, 640);
        private Rect _textureRect;

        // UI Style
        private GUIStyle _miniTitleStyle;
        private GUIStyle _boxStyle;

        // Control Parameters
        private float _rotationY = 0f;
        private float _animationSpeed = 0.5f;
        private bool _isPlaying = true;
        private bool _isScreenshot = false;

        // Setting
        private string _savePath;

        private bool _isLoaded;

        public static void Open(GameObject modelPrefab, MotionData motionData)
        {
            MotionPreviewWindow window = GetWindow<MotionPreviewWindow>("Motion Preview");
            window.titleContent = new GUIContent("Motion Preview");
            window.minSize = new Vector2(640, 900);
            window.maxSize = new Vector2(640, 900);

            window.ResetPreview();
            window.Initialize(modelPrefab, motionData);

            window.ShowUtility();
        }

        private void ResetPreview()
        {
            if (!_isLoaded) return;

            // Clear the previous preview GameObject
            if (_previewGO != null)
            {
                DestroyImmediate(_previewGO);
                _previewGO = null;
            }

            // Clear previous Animator and Controller references
            _animator = null;
            _animatorController = null;
            _previewState = null;

            // Dispose of the old PreviewRenderUtility
            if (_previewRenderUtility != null)
            {
                _previewRenderUtility.Cleanup();
                _previewRenderUtility = null;
            }

            // Create a new PreviewRenderUtility instance
            _previewRenderUtility = new PreviewRenderUtility();

            // Reset rotation and playback parameters
            _rotationY = 0f;
            _animationSpeed = 0.5f;
            _isPlaying = true;
            _isScreenshot = false;

            // Reset the preview texture area
            _textureRect = new Rect(0, 0, _previewSize.x, _previewSize.y);

            // Unsubscribe from previous MotionData event
            if (_motionData != null)
            {
                _motionData.OnAnimationClipChange -= OnAnimationClipChanged;
            }
        }

        private void OnEnable()
        {
            _previewRenderUtility = new PreviewRenderUtility();

            _savePath = SavePathManager.GetSavePath();
        }

        private void OnDisable()
        {
            _previewRenderUtility?.Cleanup();

            if (_previewGO != null)
            {
                DestroyImmediate(_previewGO);
            }

            _isLoaded = false;
        }

        private void InitializeGUIStyle()
        {
            if (_miniTitleStyle == null)
            {
                _miniTitleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
                _miniTitleStyle.normal.textColor = Color.white;
            }

            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.normal.background = Texture2D.blackTexture;
                _boxStyle.padding = new RectOffset(2, 2, 2, 2);
            }
        }

        private void Initialize(GameObject modelPrefab, MotionData motionData)
        {
            _isLoaded = true;

            InitializeGUIStyle();

            _motionData = motionData;

            // Instantiate Model
            _previewGO = _previewRenderUtility.InstantiatePrefabInScene(modelPrefab);
            _previewGO.transform.position = new Vector3(0, -0.5f, 0);
            _previewRenderUtility.AddSingleGO(_previewGO);

            // Setting Preview Camera
            _previewRenderUtility.camera.farClipPlane = 5000;
            _previewRenderUtility.camera.transform.position = new Vector3(0, 0.5f, 10);
            _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(0, 180, 0);
            _previewRenderUtility.camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            _previewRenderUtility.camera.clearFlags = CameraClearFlags.SolidColor;

            // Setup Animator (with initial clip)
            EnsureAnimatorReady(_motionData.AnimationClip);

            _textureRect = new Rect(0, 0, _previewSize.x, _previewSize.y);

            _motionData.OnAnimationClipChange += OnAnimationClipChanged;
        }

        public void OnAnimationClipChanged()
        {
            EnsureAnimatorReady(_motionData.AnimationClip);

            // Refresh Animation
            _animator.Rebind();
            _animator.Update(0);
            _animator.Play("Preview", 0, 0f);
            _animator.Update(0);
        }

        private void EnsureAnimatorReady(AnimationClip clip = null)
        {
            if (_previewGO == null) return;

            if (!_previewGO.TryGetComponent(out _animator))
            {
                _animator = _previewGO.AddComponent<Animator>();
            }

            if (_animatorController == null)
            {
                _animatorController = new AnimatorController();
                _animatorController.AddLayer("Base Layer");

                var stateMachine = _animatorController.layers[0].stateMachine;
                _previewState = stateMachine.AddState("Preview");
                stateMachine.defaultState = _previewState;

                _animator.runtimeAnimatorController = _animatorController;
            }

            if (clip != null)
            {
                _previewState.motion = clip;
            }

            // Start Preview
            if (_animator.runtimeAnimatorController != null)
            {
                _animator.Rebind();
                _animator.Update(0);
                _animator.Play("Preview", 0, 0f);
            }
        }


        private void OnGUI()
        {
            UpdateCameraRotation();

            // Add Animtion Name
            GUILayout.Space(5);
            GUILayout.Label($"Animation Name : {_motionData.MotionName}", EditorStyles.boldLabel);
            GUILayout.Space(5);

            RenderPreview();
            DrawRotationSlider();
            DrawSpeedSlider();
            DrawPlayToggle();
            DrawSaveSettingsAndScreenshot();

            Repaint();
        }

        private void UpdateCameraRotation()
        {
            if (_previewGO == null) return;

            var cam = _previewRenderUtility.camera;

            // Calculate the combined bounds of all renderers in the preview object
            Bounds bounds = new Bounds(_previewGO.transform.position, Vector3.zero);
            foreach (var renderer in _previewGO.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            Vector3 center = bounds.center;
            float distance = bounds.size.magnitude * 2f;

            // Calculate the camera's position based on rotation around the model
            Vector3 offset = Quaternion.Euler(0, _rotationY, 0) * new Vector3(0, bounds.extents.y * 0.3f, -distance * 2.0f);

            // Set camera position and orientation
            cam.transform.position = center + offset;
            cam.transform.LookAt(center);
        }

        private void RenderPreview()
        {
            if (_isScreenshot) return;

            if (_animator != null && _isPlaying)
            {
                _animator.speed = _animationSpeed;
                _animator.Update(Time.deltaTime);
            }

            // Render Preview
            _previewRenderUtility.BeginPreview(_textureRect, GUIStyle.none);
            _previewRenderUtility.camera.Render();
            Texture tex = _previewRenderUtility.EndPreview();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Preview Frame
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Box(tex, GUIStyle.none, GUILayout.Width(tex.width), GUILayout.Height(tex.height));
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawRotationSlider()
        {
            _rotationY = DrawLabeledSlider(
                "Model Rotation",
                _rotationY,
                0f,
                360f,
                "°",
                0f,
                "Reset Rotation"
            );
        }

        private void DrawSpeedSlider()
        {
            _animationSpeed = DrawLabeledSlider(
                "Animation Speed", 
                _animationSpeed, 
                0f, 
                2f, 
                "x", 
                1f, 
                "Reset Speed"
            );
        }

        private void DrawPlayToggle()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Animation Control", GUILayout.Width(120));

            if (GUILayout.Button(_isPlaying ? "Stop" : "Play", GUILayout.Width(100)))
            {
                _isPlaying = !_isPlaying;

                if (_isPlaying)
                {
                    _animator.Play(0);
                }
                else
                {
                    _animator.speed = 0f;
                }
            }

            GUILayout.EndHorizontal();
        }

        private float DrawLabeledSlider(string label, float value, float min, float max, string unit, float resetValue, string resetLabel)
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            GUILayout.Label(label, GUILayout.Width(100));
            value = GUILayout.HorizontalSlider(value, min, max);
            GUILayout.Label($"{value:F2}{unit}", GUILayout.Width(50));

            if (GUILayout.Button(resetLabel, GUILayout.Width(120)))
            {
                value = resetValue;
            }

            GUILayout.EndHorizontal();
            return value;
        }

        private void DrawSaveSettingsAndScreenshot()
        {
            GUILayout.Space(10);

            // Section title
            EditorGUILayout.LabelField("Save Path", _miniTitleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.BeginVertical("box");

            // Save path field and browse button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(_savePath);
            if (GUILayout.Button("Browse", GUILayout.MaxWidth(80)))
            {
                _savePath = SavePathManager.BrowseSavePath();
            }

            // Take Screenshot Button
            if (GUILayout.Button("Take Screenshot", GUILayout.MinWidth(120)))
            {
                _isScreenshot = true;

                if (string.IsNullOrEmpty(_savePath))
                {
                    _savePath = EditorUtility.SaveFolderPanel(
                            "Path to Save Images",
                            _savePath,
                            Application.dataPath
                    );
                }

                CaptureScreenshot();
            }

            EditorGUILayout.EndHorizontal();

            // Info message
            EditorGUILayout.HelpBox("Choose the folder where screenshots will be saved.", MessageType.Info);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Executes the screenshot capture process
        /// </summary>
        public void CaptureScreenshot()
        {
            Texture2D tex = CapturePreviewTexture();

            string fileName = ScreenshotCapturer.Capture(_savePath, tex);

            if (!string.IsNullOrEmpty(fileName))
            {
                ShowNotification(new GUIContent("Screenshot taken!"));

                Application.OpenURL("file://" + fileName);

                _isScreenshot = false;
            }
        }

        /// <summary>
        /// Capture Preview Texture
        /// </summary>
        /// <returns>Preview Texture2D</returns>
        private Texture2D CapturePreviewTexture()
        {
            RenderTexture rt = _previewRenderUtility.camera.targetTexture;
            return ScreenshotCapturer.RenderTextureToTexture2D(rt);
        }
    }
}

#endif