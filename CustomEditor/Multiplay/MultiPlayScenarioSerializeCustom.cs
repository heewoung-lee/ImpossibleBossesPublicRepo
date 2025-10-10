using System.Collections.Generic;
using System.Reflection;
using CustomEditor.Interfaces;
using Scene;
using Scene.GamePlayScene;
using Unity.Multiplayer;
using UnityEditor;
using UnityEngine;

namespace CustomEditor.Multiplay
{
    public static class MultiPlayScenarioSerializeCustom
    {
        public static void SetUseMultiMode(bool useMultiMode, IMultiTestScene multiTestscene)
        {
            MultiPlayScenarioReflectionCustom.SetUseMultiMode(useMultiMode, multiTestscene);
            //어차피 똑같은 로직이라 공유해서 씀
        }


        public static void UpdateTag(IMultiTestScene multiTestscene)
        {
            SerializedObject so = new SerializedObject(multiTestscene.GetPlayScenarioSO());
            SerializedProperty mainEditInstance = so.FindProperty("m_MainEditorInstance"); //m_MainEditorInstance꺼내오기
            SerializedProperty editorlist = so.FindProperty("m_EditorInstances"); //리스트 꺼내오기


            List<MultiTestPlayerInfo> playerList = multiTestscene.GetMultiTestPlayers();
            for (int i = 0; i < playerList.Count; i++)
            {
                string mainEditorInstancestring = playerList[i].GetTagInfo().ToString();
                if (i == 0)
                {
                    mainEditInstance.FindPropertyRelative("m_PlayerTag").stringValue = mainEditorInstancestring;
                }
                else
                {
                    SerializedProperty element = editorlist.GetArrayElementAtIndex(i - 1);
                    //i-1의 이유는 1번 플레이어는 메인구조라 m_MainEditorInstance에서 설정해야하고 , 나머지 2,3,4번은 List필드인 m_EditorInstances에서관리
                    element.FindPropertyRelative("m_PlayerTag").stringValue = mainEditorInstancestring;
                }

                so.ApplyModifiedProperties();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                ActiveEditorTracker.sharedTracker.ForceRebuild(); //인스펙터 업데이트가 안돼서 넣었음.
            }
        }

        public static void SyncScenarioFromInspector(IMultiTestScene multiTestscene)
        {
            SerializedObject so = new SerializedObject(multiTestscene.GetPlayScenarioSO());
            SerializedProperty playerSerializeList = so.FindProperty("m_EditorInstances");
            SerializedProperty mainPlaterSerialize = so.FindProperty("m_MainEditorInstance");

            List<MultiTestPlayerInfo> playerList = multiTestscene.GetMultiTestPlayers();
            playerSerializeList.ClearArray();
            playerSerializeList.arraySize = playerList.Count - 1;

            for (int i = 0; i < playerList.Count; i++)
            {
                string mainEditorInstancestring = playerList[i].GetTagInfo().ToString();
                if (i == 0)
                {
                    mainPlaterSerialize.FindPropertyRelative("m_PlayerTag").stringValue = mainEditorInstancestring;
                }
                else
                {
                    SerializedProperty playerSerialize = playerSerializeList.GetArrayElementAtIndex(i - 1);
                    playerSerialize.FindPropertyRelative("Name").stringValue = $"Player{i + 1}";
                    playerSerialize.FindPropertyRelative("m_PlayerTag").stringValue = mainEditorInstancestring;


                    SerializedProperty advanceConfigOption =
                        playerSerialize.FindPropertyRelative("m_AdvancedConfiguration");
                    Debug.Assert(advanceConfigOption != null, "advanceConfigOption is null");

                    var pStream = advanceConfigOption.FindPropertyRelative("StreamLogsToMainEditor");
                    Debug.Assert(pStream != null, "pStream is null");
                    pStream.boolValue = false;
                    
                    var pColor = advanceConfigOption.FindPropertyRelative("LogsColor");
                    Debug.Assert(pColor != null, "pColor is null");
                    pColor.colorValue = new Color(0.3643f, 0.581f, 0.8679f);

                    SerializedProperty multiplayerRoleFlag = playerSerialize.FindPropertyRelative("m_Role");
                    Debug.Assert(multiplayerRoleFlag != null, "multiplayerRoleFlag is null");
                    multiplayerRoleFlag.intValue = (int)MultiplayerRoleFlags.Client;
                }
            }

            so.ApplyModifiedProperties();
            ActiveEditorTracker.sharedTracker.ForceRebuild();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}