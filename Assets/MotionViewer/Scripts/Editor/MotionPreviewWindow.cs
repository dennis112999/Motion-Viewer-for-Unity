using UnityEngine;

using UnityEditor;

namespace Dennis.Tools.MotionViewer
{
    /// <summary>
    /// Motion Preview Window
    /// </summary>
    public class MotionPreviewWindow : EditorWindow
    {
        // Preview: Model & Animation
        private PreviewRenderUtility _previewRenderUtility;
        private GameObject _previewGO;
        private Animator _animator;
        private RuntimeAnimatorController _controller;

        // Preview Settings
        private Vector2 _previewSize = new Vector2(640, 640);
        private Rect _textureRect;

        // UI Style
        private GUIStyle _boxStyle;

        // Control Parameters
        private float _rotationY = 0f;
        private float _animationSpeed = 1f;
        private bool _isPlaying = true;

        public static void Open(GameObject modelPrefab, RuntimeAnimatorController clip)
        {
            var window = CreateInstance<MotionPreviewWindow>();
            window.titleContent = new GUIContent("Motion Preview");
            window.maxSize = new Vector2(640, 1500);
            window.Initialize(modelPrefab, clip);
            window.ShowUtility();
        }

        private void OnEnable()
        {
            _previewRenderUtility = new PreviewRenderUtility();
        }

        private void OnDisable()
        {
            _previewRenderUtility?.Cleanup();

            if (_previewGO != null)
            {
                DestroyImmediate(_previewGO);
            }
        }

        private void InitializeGUIStyle()
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.normal.background = Texture2D.blackTexture;
                _boxStyle.padding = new RectOffset(2, 2, 2, 2);
            }
        }

        private void Initialize(GameObject modelPrefab, RuntimeAnimatorController controller)
        {
            InitializeGUIStyle();

            _controller = controller;

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

            // Setting Animator
            if (!_previewGO.TryGetComponent<Animator>(out _animator))
            {
                _animator = _previewGO.AddComponent<Animator>();
            }

            _animator.runtimeAnimatorController = _controller;

            _textureRect = new Rect(0, 0, _previewSize.x, _previewSize.y);
        }

        private void OnGUI()
        {
            RotationModel();

            // Add Animtion Name
            GUILayout.Space(5);
            GUILayout.Label($"Animation Name : {_controller.name}", EditorStyles.boldLabel);
            GUILayout.Space(5);

            RenderPreview();
            DrawRotationSlider();
            DrawSpeedSlider();
            DrawPlayToggle();

            Repaint();
        }

        private void RotationModel()
        {
            if (_previewGO == null) return;
            _previewGO.transform.rotation = Quaternion.Euler(0, _rotationY, 0);
        }

        private void RenderPreview()
        {
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
    }
}
