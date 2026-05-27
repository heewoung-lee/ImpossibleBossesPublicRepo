#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CustomEditor
{
    public static class EmptyFolderCleaner
    {
        [MenuItem("Utility/Select Empty Folders")]
        private static void SelectEmptyFolders()
        {
            AssetDatabase.Refresh();

            List<string> emptyFolders = FindEmptyFolders();
            List<Object> folderObjects = new List<Object>();

            foreach (string folder in emptyFolders)
            {
                Object folderObject = AssetDatabase.LoadAssetAtPath<Object>(folder);
                if (folderObject != null)
                    folderObjects.Add(folderObject);
            }

            Selection.objects = folderObjects.ToArray();
            EditorUtility.DisplayDialog("Empty Folders", $"Found {folderObjects.Count} empty folders.", "OK");
        }

        [MenuItem("Utility/Delete Empty Folders")]
        private static void DeleteEmptyFolders()
        {
            AssetDatabase.Refresh();

            List<string> emptyFolders = FindEmptyFolders();
            if (emptyFolders.Count == 0)
            {
                EditorUtility.DisplayDialog("Delete Empty Folders", "No empty folders found.", "OK");
                return;
            }

            bool shouldDelete = EditorUtility.DisplayDialog(
                "Delete Empty Folders",
                $"Delete {emptyFolders.Count} empty folders under Assets?\nThe matching .meta files will be removed too.",
                "Delete",
                "Cancel");

            if (!shouldDelete)
                return;

            int deletedCount = 0;
            int failedCount = 0;

            foreach (string folder in emptyFolders)
            {
                if (AssetDatabase.DeleteAsset(folder))
                    deletedCount++;
                else
                    failedCount++;
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Delete Empty Folders",
                $"Deleted {deletedCount} empty folders.\nFailed: {failedCount}",
                "OK");
        }

        private static List<string> FindEmptyFolders()
        {
            List<string> emptyFolders = new List<string>();
            CollectEmptyFolders("Assets", emptyFolders);
            emptyFolders.Sort((left, right) => right.Length.CompareTo(left.Length));
            return emptyFolders;
        }

        private static bool CollectEmptyFolders(string folderPath, List<string> emptyFolders)
        {
            string[] subFolders = AssetDatabase.GetSubFolders(folderPath);
            bool hasNonEmptySubFolder = false;

            foreach (string subFolder in subFolders)
            {
                if (!CollectEmptyFolders(subFolder, emptyFolders))
                    hasNonEmptySubFolder = true;
            }

            bool hasDirectAsset = HasDirectAssetFile(folderPath);
            bool isEmpty = !hasDirectAsset && !hasNonEmptySubFolder;

            if (isEmpty && folderPath != "Assets")
                emptyFolders.Add(folderPath);

            return isEmpty;
        }

        private static bool HasDirectAssetFile(string folderPath)
        {
            string absoluteFolderPath = Path.Combine(GetProjectRoot(), folderPath);

            foreach (string filePath in Directory.GetFiles(absoluteFolderPath))
            {
                if (!filePath.EndsWith(".meta"))
                    return true;
            }

            return false;
        }

        private static string GetProjectRoot()
        {
            return Directory.GetParent(Application.dataPath).FullName;
        }
    }
}
#endif
