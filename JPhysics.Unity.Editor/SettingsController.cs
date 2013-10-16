namespace JPhysics.Unity.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    class SettingsController
    {
        const string Path = "Assets/Resources/JSettings.asset";

        static SettingsController()
        {
            EditorApplication.projectWindowChanged += ExistSettings;
            EditorApplication.hierarchyWindowChanged += ExistSettings;
            EditorApplication.playmodeStateChanged += ExistSettings;
            EditorApplication.update += CheckPause;
        }

        private static void CheckPause()
        {
            if (JPhysics.IsInstance)
            {
                var i = JPhysics.Instance;
                if (EditorApplication.isPlaying)
                {
                    if (i.IsPaused != EditorApplication.isPaused)
                    {
                        i.IsPaused = !i.IsPaused;
                    }
                }
                else Object.DestroyImmediate(i);
            }
        }


        private static void ExistSettings()
        {
            var set = AssetDatabase.LoadAssetAtPath(Path, typeof(JSettings)) as JSettings;
            if (set == null)
            {
                var path = Application.dataPath + "/Resources";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                set = ScriptableObject.CreateInstance<JSettings>();
                //set.hideFlags = HideFlags.NotEditable;
                AssetDatabase.CreateAsset(set, Path);
                AssetDatabase.Refresh();
            }
        }
    }
}
