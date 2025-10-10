using UnityEditor;
using UnityEngine;
using Util;

#if UNITY_EDITOR
namespace Scene.RoomScene
{
    [UnityEditor.CustomEditor(typeof(RoomPlayScene))]
    public class RoomSceneInspectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
        
            // ── 상위 라디오 ─────────────────────────────────
            SerializedProperty modeProp =
                serializedObject.FindProperty("testMode");
        
            if (modeProp.enumValueIndex < 0 ||
                modeProp.enumValueIndex >= modeProp.enumDisplayNames.Length)
            {
                modeProp.enumValueIndex = 0;   // 기본값(Local)로 강제
            }
            
            EditorGUILayout.LabelField("Test Mode", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            modeProp.enumValueIndex =
                GUILayout.Toggle(modeProp.enumValueIndex == 0, "Local",  EditorStyles.radioButton) ? 0 : modeProp.enumValueIndex;
            modeProp.enumValueIndex =
                GUILayout.Toggle(modeProp.enumValueIndex == 1, "Multi",  EditorStyles.radioButton) ? 1 : modeProp.enumValueIndex;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        
            // ── 하위 라디오: testMode == Multi 일 때만 ─────────
            if (modeProp.enumValueIndex == (int)TestMode.Multi)
            {
                SerializedProperty subProp =
                    serializedObject.FindProperty("multiMode");
        
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Multi Option", EditorStyles.boldLabel);
                subProp.enumValueIndex =
                    GUILayout.Toggle(subProp.enumValueIndex == 0, "Solo",  EditorStyles.radioButton) ? 0 : subProp.enumValueIndex;
                subProp.enumValueIndex =
                    GUILayout.Toggle(subProp.enumValueIndex == 1, "Multi", EditorStyles.radioButton) ? 1 : subProp.enumValueIndex;
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
