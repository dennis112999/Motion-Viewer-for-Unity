using System.Linq;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Dennis.Tools.MotionViewer
{
    public class MotionViewerWindow : UnityEditor.EditorWindow
    {
        private MotionViewerSO _motionViewerSO;
        private GameObject _modelPrefab;
        public GameObject ModelPrefab
        {
            get { return _modelPrefab; }
            private set { _modelPrefab = value; }
        }

        private ScrollView _motionListView;

        private MotionSaveUtility _motionSaveUtility;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [OnOpenAsset(0)]
        public static bool OpenMotionViewerSO(int instanceID, int line)
        {
            UnityEngine.Object item = EditorUtility.InstanceIDToObject(instanceID);

            if (item is MotionViewerSO motionViewerSO)
            {
                // Make a unity editor window of type MotionViewerWindow
                MotionViewerWindow window = (MotionViewerWindow)GetWindow(typeof(MotionViewerWindow));
                window.titleContent = new GUIContent("Motion Viewer");
                window.minSize = new Vector2(500, 250);

                // Load in MotionViewerSO data in to the editor window
                window.Load(motionViewerSO);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Will load in current selected MotionViewerSO
        /// </summary>
        private void Load(MotionViewerSO motionViewerSO)
        {
            _motionSaveUtility = MotionSaveUtility.GetInstance();
            _motionViewerSO = _motionSaveUtility.LoadMotionView(motionViewerSO);

            RefreshMotionList();
        }

        private void OnEnable()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/MotionViewer/Resources/MotionViewer.uss");
            rootVisualElement.styleSheets.Add(styleSheet);

            GenerateToolbar();
            GenerateScrollView();
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            // Add Save Data Button
            Button SaveDataButton = UIHelper.CreateButton("Save Data", () => SaveData());
            toolbar.Add(SaveDataButton);

            Button LoadDataButton = UIHelper.CreateButton("Load Data", () => LoadData());
            toolbar.Add(LoadDataButton);

            // Add Clear Button
            Button ClearButton = UIHelper.CreateButton("Clear", () => ClearMotionListView());
            toolbar.Add(ClearButton);

            toolbar.Add(new ToolbarSpacer { style = { flexGrow = 1 } });

            // Add GameObject ObjectField
            ObjectField ModelObjectField = UIHelper.CreateObjectField<GameObject>(
               _modelPrefab,
               (newGameObject) => { _modelPrefab = newGameObject; }
            );
            toolbar.Add(ModelObjectField);

            // Add Add motion Button
            Button AddMotionButton = UIHelper.CreateButton("Add Motion", () => AddMotionData(), "motion-add-button");
            toolbar.Add(AddMotionButton);

            rootVisualElement.Add(toolbar);
        }

        private void GenerateScrollView()
        {
            _motionListView = new ScrollView();
            _motionListView.style.flexGrow = 1;
            _motionListView.style.marginTop = 5;
            rootVisualElement.Add(_motionListView);
        }

        private void RefreshMotionList()
        {
            _motionListView.Clear();

            if (_motionViewerSO == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[MotionViewerWindow] Missing MotionViewerSO. Please assign a valid reference.");
#endif
                return;
            }

            foreach (MotionData motion in _motionViewerSO.MotionDatas)
            {
                MotionItemElement item = new MotionItemElement(motion, RemoveMotionData);
                _motionListView.Add(item);
            }
        }

        private void AddMotionData()
        {
            MotionData newMotionData = _motionViewerSO.AddMotionData();

            MotionItemElement item = new MotionItemElement(newMotionData, RemoveMotionData);
            _motionListView.Add(item);
        }

        private void RemoveMotionData(MotionData data)
        {
            _motionViewerSO.RemoveMotionData(data);

            var toRemove = _motionListView.Children().OfType<MotionItemElement>()
                    .FirstOrDefault(e => e.MotionData == data);

            if (toRemove != null)
                _motionListView.Remove(toRemove);
        }

        #region Save Load

        private void SaveData()
        {
            _motionSaveUtility.SaveMotionView(_motionViewerSO);
        }

        private void LoadData()
        {
            _motionViewerSO = _motionSaveUtility.LoadMotionView();

            RefreshMotionList();
        }

        #endregion Save Load

        private void ClearMotionListView()
        {
            _motionListView.Clear();
        }
    }

}