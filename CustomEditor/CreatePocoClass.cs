using System.IO;
using UnityEditor;
using UnityEngine;

namespace CustomEditor
{
    public class CreatePocoClass
    {
        [MenuItem("Assets/Create/C# PocoClass %#o", false, 80)]
        private static void CreateInterface()
        {
            string templatePath = "Assets/Scripts/CustomEditor/ScriptTemplates/PocoClassTemplate.txt";
            string defaultName = "NewPocoClass.cs";

            if (!File.Exists(templatePath))
                Debug.LogError("Template NOT found!");

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, defaultName);
        }
    }
}