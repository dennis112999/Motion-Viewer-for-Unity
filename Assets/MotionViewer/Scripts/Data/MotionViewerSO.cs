using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dennis.Tools.MotionViewer
{
    [Serializable]
    public class MotionData
    {
        public Sprite MotionPhoto;
        public string MotionName;
        public string Description;
        public AnimationClip AnimationClip;

        public Action OnAnimationClipChange;

        public void SetAnimationClip(AnimationClip newAnimationClip)
        {
            AnimationClip = newAnimationClip;
            OnAnimationClipChange?.Invoke();
        }

        public MotionData(string motionName, string desc, AnimationClip animation = null, Sprite motionPhoto = null)
        {
            MotionPhoto = motionPhoto;
            MotionName = motionName;
            Description = desc;
            AnimationClip = animation;
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "MotionViewerSO", menuName = "Data/MotionViewerSO")]
    public class MotionViewerSO : ScriptableObject
    {
        public List<MotionData> MotionDatas { get; private set; } = new List<MotionData>();

        public MotionData AddMotionData()
        {
            MotionData motionData = new MotionData("Motion Name", "Motion Description");
            MotionDatas.Add(motionData);

            return motionData;
        }

        public MotionData AddMotionData(MotionData motionData)
        {
            MotionDatas.Add(motionData);

            return motionData;
        }

        public void RemoveMotionData(MotionData motionData)
        {
            if (MotionDatas.Contains(motionData))
            {
                MotionDatas.Remove(motionData);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[MotionViewerSO] Failed to remove: MotionData instance not found.\nName: {motionData?.MotionName ?? "null"}");
#endif
            }
        }

        public void ClearMotionData()
        {
            MotionDatas.Clear();
        }
    }
}