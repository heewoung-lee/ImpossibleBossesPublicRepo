using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomEditor.Interfaces;
using Scene;
using Scene.GamePlayScene;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;


namespace CustomEditor.Multiplay
{
    
    public static class MultiPlayScenarioReflectionCustom
    {
        private static Type _playModeManger;
        private static PropertyInfo _activePlayModeConfig;
        private static Type _virtualEditorInstanceDescription;
        private static object _playModeMangerInstance;
        private static object _defaultConfigInstance;
        
        
        private static Dictionary<IMultiTestScene, ScriptableObject> _scenarios =
            new Dictionary<IMultiTestScene, ScriptableObject>();
        
        
        private static Dictionary<Type,Dictionary<(string,BindingFlags),MemberInfo>> _fieldInfos 
            = new Dictionary<Type,Dictionary<(string,BindingFlags),MemberInfo>>();
        
        
        public static Type PlayModeManger
        {
            get
            {
                if (_playModeManger == null)
                {
                    _playModeManger = Type.GetType("Unity.Multiplayer.PlayMode.Configurations.Editor.PlayModeManager, Unity.Multiplayer.PlayMode.Configurations.Editor");
                }
                return _playModeManger;
            }
        }

        public static PropertyInfo ActivePlayModeConfig
        {
            get
            {
                if (_activePlayModeConfig == null)
                {
                    _activePlayModeConfig = GetOrRegisterCachingType<PropertyInfo>
                        (PlayModeManger,"ActivePlayModeConfig",BindingFlags.Public| BindingFlags.Instance);
                }
                return _activePlayModeConfig;
            }
        }

        public static object PlayModeManagerInstance
        {
            get
            {
                if (_playModeMangerInstance == null)
                {
                    Type playModeManagerType    = PlayModeManger; // 이미 캐시해둔 Type
                    Type genericofPlayModeManagerType =
                        typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(playModeManagerType);
            
                    PropertyInfo instProp  = GetOrRegisterCachingType<PropertyInfo>(
                        genericofPlayModeManagerType, "instance",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    
                    _playModeMangerInstance = instProp.GetValue(null);
                }
                return _playModeMangerInstance;
            }
        }

        public static object DefaultPlayModeConfig
        {
            get
            {
                if (_defaultConfigInstance == null)
                {
                    PropertyInfo propertyInfo =  GetOrRegisterCachingType<PropertyInfo>(PlayModeManger,
                        "DefaultConfig",BindingFlags.Public| BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                    _defaultConfigInstance = propertyInfo.GetValue(PlayModeManagerInstance);
                }
                return _defaultConfigInstance;
            }
            
        }

        public static void SetUseMultiMode(bool useMultiMode,IMultiTestScene multiTestscene)
        {
            if (multiTestscene == null)
            {
                Debug.LogError("Multiplay scene cannot be null");
                return;
            }

            ScriptableObject currentSO;
            if (useMultiMode == true)  //멀티모드를 사용할껀지 
            {
                currentSO = LoadScenarioSO(multiTestscene);
            }
            else
            {
                currentSO = (ScriptableObject)DefaultPlayModeConfig;
            }
            ActivePlayModeConfig.SetValue(PlayModeManagerInstance,currentSO);
            
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            EditorApplication.QueuePlayerLoopUpdate();
            
        }

        private static T GetOrRegisterCachingType<T>(Type type,string name,BindingFlags flags) where T : System.Reflection.MemberInfo
        {
            MemberInfo memberInfo = null;

            if (_fieldInfos.TryGetValue(type, out Dictionary<(string, BindingFlags), MemberInfo> memberInfos) == false)
            {
                memberInfos = new Dictionary<(string, BindingFlags), MemberInfo>();
                _fieldInfos[type] = memberInfos;
            }
            
            
            if (memberInfos.TryGetValue((name, flags), out memberInfo) == false)
            {
                if (typeof(T) == typeof(FieldInfo))
                {
                    memberInfo = type.GetField(name, flags);
                    if (memberInfo == null)
                    {
                        Debug.Log($"{type.Name} scenario {typeof(T).Name} didn't found {name} flag{flags} ");    
                        return null;
                    }
                    memberInfos.Add((name,flags),memberInfo); //중복되면 안되니깐 Add로 선언
                }
                else if (typeof(T) == typeof(MethodInfo))
                {
                    memberInfo = type.GetMethod(name, flags);
                    if (memberInfo == null)
                    {
                        Debug.Log($"{type.Name} scenario {typeof(T).Name} didn't found {name} flag{flags} ");    
                        return null;
                    }
                    memberInfos.Add((name,flags),memberInfo); //중복되면 안되니깐 Add로 선언
                }
                else if (typeof(T) == typeof(PropertyInfo))
                {
                    memberInfo = type.GetProperty(name, flags);
                    if (memberInfo == null)
                    {
                        Debug.Log($"{type.Name} scenario {typeof(T).Name} didn't found {name} flag{flags} ");    
                        return null;
                    }
                    memberInfos.Add((name,flags),memberInfo); //중복되면 안되니깐 Add로 선언
                }
                else
                    throw new NotSupportedException($"Unsupported MemberInfo type: {typeof(T).Name}");
                
                return memberInfo as T;
            }
            else
            {
                return memberInfo as T;
            }
        }

        private static ScriptableObject LoadScenarioSO(IMultiTestScene multiTestscene)
        {
            ScriptableObject currentScenario = null;

            if (_scenarios.TryGetValue(multiTestscene, out var scenario) == false) //만약 저장되어있는 현재씬이 없다면.
            {
                ScriptableObject playScenario = multiTestscene.GetPlayScenarioSO();
                _scenarios.Add(multiTestscene, playScenario);
                currentScenario = playScenario;
            }
            else
            {
                currentScenario = scenario;
            }

            Debug.Assert(currentScenario != null, nameof(currentScenario) + " is null");
            return currentScenario;
        }

      
        public static void UpdateTag(IMultiTestScene multiTestscene)
        {
            ScriptableObject currentScenario = LoadScenarioSO(multiTestscene);
          
            Type currentScenarioType = currentScenario.GetType();
            FieldInfo listField = GetOrRegisterCachingType<FieldInfo>(currentScenarioType,"m_EditorInstances", BindingFlags.Instance | BindingFlags.NonPublic);

            Debug.Assert(listField != null, "listField == null");
            object listobj = listField.GetValue(currentScenario);
            Debug.Assert(listobj != null, "listobj == null");

            IList list = listobj as IList;
            Debug.Assert(list != null, "list == null");

            //현재 리스트 가져옴.
            List<MultiTestPlayerInfo> playerList = multiTestscene.GetMultiTestPlayers();
            for (int i = 0; i < playerList.Count; i++)
            {
                if (i == 0)
                {
                    Type mainEditorInstance = currentScenario.GetType();
                    object editorObj = GetOrRegisterCachingType<FieldInfo>
                        (mainEditorInstance,"m_MainEditorInstance", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(currentScenario);
                    
                    Debug.Assert(editorObj != null, "EditorInstance is null");
                    string mainEditorInstancestring = playerList[i].GetTagInfo().ToString();
                    SetMember(editorObj, "PlayerTag", mainEditorInstancestring);
                }
                else
                {
                    object item = list[i - 1];
                    SetMember(item, "PlayerTag", playerList[i].GetTagInfo().ToString());
                }
            }

            EditorUtility.SetDirty(currentScenario);
            AssetDatabase.SaveAssets();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            ActiveEditorTracker.sharedTracker.ForceRebuild();//인스펙터 업데이트가 안돼서 넣었음.
        }


        public static void SyncScenarioFromInspector(IMultiTestScene multiTestscene)
        {
            ScriptableObject currentScenario = LoadScenarioSO(multiTestscene);
            ScenarioAsset();

            void ScenarioAsset()
            {
                Type scenariotype = currentScenario.GetType();
                FieldInfo listField = GetOrRegisterCachingType<FieldInfo>(scenariotype, "m_EditorInstances",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                
                Debug.Assert(listField != null, "listField == null");

                object listobj = listField.GetValue(currentScenario);
                Debug.Assert(listobj != null, "listobj == null");

                IList list = listobj as IList;
                Debug.Assert(list != null, "list == null");

                if (_virtualEditorInstanceDescription == null)
                {
                    _virtualEditorInstanceDescription = GetListElementType(listobj.GetType());
                }

                Debug.Assert(_virtualEditorInstanceDescription != null, "elemType == null");


                List<MultiTestPlayerInfo> playerList = multiTestscene.GetMultiTestPlayers();
                list.Clear();

                for (int i = 0; i < playerList.Count; i++)
                {
                    if (i == 0)
                    {
                        var mainEditorInstance = currentScenario.GetType();

                        object editorObj = GetOrRegisterCachingType<FieldInfo>
                            (mainEditorInstance, "m_MainEditorInstance",
                                BindingFlags.NonPublic | BindingFlags.Instance)
                            .GetValue(currentScenario);

                        Debug.Assert(editorObj != null, "EditorInstance is null");
                        string mainEditorInstancestring = playerList[i].GetTagInfo().ToString();

                        SetMember(editorObj, "PlayerTag", mainEditorInstancestring);
                    }
                    else
                    {
                        object newElem = Activator.CreateInstance(_virtualEditorInstanceDescription);

                        FieldInfo newName = GetOrRegisterCachingType<FieldInfo>(newElem.GetType(), "Name", BindingFlags.Instance | BindingFlags.Public);
                        
                        SetMember(newElem, "Name", $"Player{i + 1}");
                        SetMember(newElem, "PlayerTag", playerList[i].GetTagInfo().ToString());
                        list.Add(newElem);
                    }
                }

                EditorUtility.SetDirty(currentScenario);
                AssetDatabase.SaveAssets();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }

        private static Type GetListElementType(Type listType)
        {
            if (listType.IsGenericType) return listType.GetGenericArguments().FirstOrDefault();
            //MethodInfo add = listType.GetMethod("Add");
            
            MethodInfo add = GetOrRegisterCachingType<MethodInfo>(listType, "Add", BindingFlags.Public);
            ParameterInfo[] ps = add?.GetParameters();
            return (ps != null && ps.Length == 1) ? ps[0].ParameterType : null;
        }

        private static void SetMember(object obj, string logicalName, object value)
        {
            if (obj == null) return;
            Type type = obj.GetType();

            // public Property 우선
            PropertyInfo propertyType = GetOrRegisterCachingType<PropertyInfo>(type, logicalName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (propertyType != null && propertyType.CanWrite)
            {
                propertyType.SetValue(obj, value);
                return;
            }
            
            FieldInfo fieldTypePublic = GetOrRegisterCachingType<FieldInfo>(type,logicalName,BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (fieldTypePublic != null)
            {
                fieldTypePublic.SetValue(obj, value);
                return;
            }
            
            //8.15일 느슨한 탐색 전부 없애버림
            #region DeleteCode Release Find
            // FieldInfo fieldType = GetOrRegisterCachingType<FieldInfo>(type,"m_" + logicalName,BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //
            // if (fieldType != null)
            // {
            //     fieldType.SetValue(obj, value);
            //     return;
            // }
            
            //
            //
            //
            // // 느슨한 검색(대소문자 무시, 부분일치)
            // propertyType = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            //     .FirstOrDefault(p =>
            //         p.Name.IndexOf(logicalName, StringComparison.OrdinalIgnoreCase) >= 0 && p.CanWrite);
            // if (propertyType != null)
            // {
            //     propertyType.SetValue(obj, value);
            //     return;
            // }
            //
            // fieldType = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            //     .FirstOrDefault(f => f.Name.IndexOf(logicalName, StringComparison.OrdinalIgnoreCase) >= 0);
            // if (fieldType != null)
            // {
            //     fieldType.SetValue(obj, value);
            // }
            #endregion
        }
    }
}