using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
namespace Scene.GamePlayScene
{
    //[UnityEditor.CustomEditor(typeof(PlayScene))]
    //8.12일 커스텀에디터를 없애고 오딘인스펙터로 인스페터 꾸미기로 결정
    //이유는 커스텀에디터를 쓰면 커스텀에디터에 대한 위치를 찾기가 어렵고, 또한 인스펙터를 만들기도 어려움.
    public class PlaySceneInspectorEditor : UnityEditor.Editor
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
            if (modeProp.enumValueIndex == (int)MultiMode.Multi)
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
            if (modeProp.enumValueIndex == (int)MultiMode.Multi)
            {
                // SerializedProperty subProp =
                //     serializedObject.FindProperty(PlayScene.PlayerableCharacterField);

                // EditorGUI.indentLevel++;
                // EditorGUILayout.LabelField("Choice Character", EditorStyles.boldLabel);
                // subProp.enumValueIndex =
                //     GUILayout.Toggle(subProp.enumValueIndex == 0, "Archer",  EditorStyles.radioButton) ? 0 : subProp.enumValueIndex;
                // subProp.enumValueIndex =
                //     GUILayout.Toggle(subProp.enumValueIndex == 1, "Fighter", EditorStyles.radioButton) ? 1 : subProp.enumValueIndex;
                // subProp.enumValueIndex =
                //     GUILayout.Toggle(subProp.enumValueIndex == 2, "Mage", EditorStyles.radioButton) ? 2 : subProp.enumValueIndex;
                // subProp.enumValueIndex =
                //     GUILayout.Toggle(subProp.enumValueIndex == 3, "Monk", EditorStyles.radioButton) ? 3 : subProp.enumValueIndex;
                // subProp.enumValueIndex =
                //     GUILayout.Toggle(subProp.enumValueIndex == 4, "Necromancer", EditorStyles.radioButton) ? 4 : subProp.enumValueIndex;
                // EditorGUI.indentLevel--;
            }
            

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
