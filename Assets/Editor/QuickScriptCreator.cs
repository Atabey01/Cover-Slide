#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using System.IO;

public class QuickScriptCreator : OdinEditorWindow
{
    [FolderPath(AbsolutePath = false)]
    [ReadOnly]
    [PropertyOrder(-1)]
    public string targetFolder = "Assets";

    [ListDrawerSettings(Expanded = true, CustomAddFunction = "AddEntry", CustomRemoveElementFunction = "RemoveEntry")]
    public List<string> scriptNames = new List<string>();

    [MenuItem("Assets/Create/Quick Scripts", false, 19)]
    private static void OpenWindow()
    {
        var window = GetWindow<QuickScriptCreator>();
        string folder = GetSelectedFolderPath();
        if (!string.IsNullOrEmpty(folder))
            window.targetFolder = folder;

        window.Show();
    }

    // + Tuşuna basıldığında boş input
    private void AddEntry()
    {
        scriptNames.Add("NewScript");
    }

    private void RemoveEntry(string name)
    {
        scriptNames.Remove(name);
    }

    [Button(ButtonSizes.Large), GUIColor(0.2f, 0.8f, 0.2f)]
    public void CreateScripts()
    {
        if (scriptNames.Count == 0) return;

        foreach (string scriptName in scriptNames)
        {
            if (string.IsNullOrWhiteSpace(scriptName)) continue;

            string path = $"{targetFolder}/{scriptName}.cs";
            if (File.Exists(path))
            {
                Debug.LogWarning($"{scriptName}.cs zaten var, geçildi.");
                continue;
            }

            string template = GetMonoBehaviourTemplate(scriptName);
            File.WriteAllText(path, template);
        }

        AssetDatabase.Refresh();
        Debug.Log("🧩 Script oluşturma tamam! Compile ediliyor...");
    }


    private static string GetSelectedFolderPath()
    {
        Object obj = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(obj);

        if (string.IsNullOrEmpty(path)) return "Assets";

        // klasör seçilmediyse dosyadan klasöre çevir
        if (!AssetDatabase.IsValidFolder(path))
        {
            path = Path.GetDirectoryName(path);
        }

        return path;
    }


    private string GetMonoBehaviourTemplate(string className)
    {
        return
$@"using UnityEngine;

public class {className} : MonoBehaviour
{{
    private void Start()
    {{
        
    }}

    private void Update()
    {{
        
    }}
}}";
    }
}
#endif
