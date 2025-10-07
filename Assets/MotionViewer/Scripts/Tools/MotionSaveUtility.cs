#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Dennis.Tools.MotionViewer
{
    /// <summary>
    /// Sample Motion Viewer SO Save Utility
    /// </summary>
    public class MotionSaveUtility
    {
        private MotionViewerSO _motionViewerSO;
        private static MotionSaveUtility _instance;

        private MotionSaveUtility() { }

        public static MotionSaveUtility GetInstance()
        {
            return _instance ??= new MotionSaveUtility();
        }

        #region Save

        public void SaveMotionView(MotionViewerSO motionViewerSO)
        {
            _motionViewerSO.ClearMotionData();

            foreach (var data in motionViewerSO.MotionDatas)
            {
                _motionViewerSO.AddMotionData(new MotionData(
                    data.MotionName,
                    data.Description,
                    data.AnimationClip,
                    data.MotionPhoto
                ));
            }

            EditorUtility.DisplayDialog(
                "Save Successful", 
                $"File '{_motionViewerSO.name}.asset' has been saved successfully!", 
                "OK"
            );
        }

        #endregion Save

        #region Load

        public MotionViewerSO LoadMotionView()
        {
            return CreateCloneFrom(_motionViewerSO);
        }

        public MotionViewerSO LoadMotionView(MotionViewerSO motionViewerSO)
        {
            _motionViewerSO = motionViewerSO;
            return CreateCloneFrom(motionViewerSO);
        }

        #endregion Load

        private MotionViewerSO CreateCloneFrom(MotionViewerSO source)
        {
            MotionViewerSO tmpSO = ScriptableObject.CreateInstance<MotionViewerSO>();
            tmpSO.name = source.name;

            foreach (var data in source.MotionDatas)
            {
                tmpSO.AddMotionData(new MotionData(
                    data.MotionName,
                    data.Description,
                    data.AnimationClip,
                    data.MotionPhoto
                ));
            }

            return tmpSO;
        }

    }
}

#endif