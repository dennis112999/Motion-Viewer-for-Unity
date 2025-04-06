#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Dennis.Tools.MotionViewer
{
    /// <summary>
    /// Manages the save path for screenshots using EditorPrefs
    /// </summary>
    public class SavePathManager
    {
        private const string EditorPrefKey = "MotionViewer_SavePath";

        public static string GetSavePath()
        {
            return EditorPrefs.GetString(EditorPrefKey, Application.dataPath);
        }

        public static string BrowseSavePath()
        {
            try
            {
                string path = EditorUtility.SaveFolderPanel("Select Save Folder", GetSavePath(), Application.dataPath);
                if (!string.IsNullOrEmpty(path))
                {
                    EditorPrefs.SetString(EditorPrefKey, path);
                }
                return path;
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to select or save folder:\n{ex.Message}", "OK");
                return GetSavePath();
            }
        }
    }
}

#endif