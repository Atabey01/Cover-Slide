using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class Tools
    {
        [MenuItem("Tools/Clear All Saves Reload Domain")]
        private static void ClearAllSavesReloadDomain()
        {
            if (Application.isPlaying)
                return;
            
            PlayerPrefs.DeleteAll();
            EditorUtility.RequestScriptReload();

            var path = $"{Application.dataPath}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}remote_config_data";
            if(File.Exists(path))
                File.Delete(path);

            if (Directory.Exists(Application.persistentDataPath))
            {
                Directory.Delete(Application.persistentDataPath, true);
            }
        }
    }
}