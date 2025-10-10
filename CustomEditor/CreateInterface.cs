using System.IO;
using UnityEditor;
using UnityEngine;

namespace CustomEditor
{
    public class CreateInterfaceTemplate
    {
        [MenuItem("Assets/Create/C# Interface %#i", false, 80)]
        private static void CreateInterface()
        {
            string templatePath = "Assets/Scripts/CustomEditor/ScriptTemplates/InterfaceTemplate.txt";
            string defaultName = "NewInterface.cs";
        
            if (!File.Exists(templatePath))
                Debug.LogError("Template NOT found!");
        
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, defaultName);
        }
    }
}