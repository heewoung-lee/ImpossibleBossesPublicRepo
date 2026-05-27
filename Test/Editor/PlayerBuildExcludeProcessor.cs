using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Test.Editor
{
    // 2026-04-11 18:37:48 +09:00
    // 문제:
    // 프로덕션 빌드에 테스트 전용 에디터 컴포넌트가 같이 들어가면서
    // Missing Script / ScenarioConfig serialization layout 경고가 발생했다.
    // 빌드 시점에는 일부 테스트 컴포넌트가 살아 있는 상태로 남고,
    // 일부는 이미 missing script 슬롯으로 변해 있어서 한쪽만 제거하면 로그가 계속 남았다.
    //
    // 해결:
    // 실제 플레이어 빌드에서만(report != null) 빌드용 씬 복사본을 순회하면서
    // 1) IExcludeFromPlayerBuild 마커가 붙은 테스트 컴포넌트를 제거하고
    // 2) 이미 missing script 로 변한 슬롯도 같이 제거한다.
    // 이렇게 해서 에디터 테스트 흐름은 유지하고, 프로덕션 빌드에서만 테스트 참조를 제외한다.
    public sealed class PlayerBuildExcludeProcessor : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            // Unity는 에디터 플레이 모드에서도 이 콜백을 호출할 수 있으므로,
            // 실제 플레이어 빌드가 아닐 때는 아무 작업도 하지 않는다.
            if (report == null)
            {
                return;
            }

            List<MonoBehaviour> buildExcludedComponents = new List<MonoBehaviour>();
            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                Transform[] transforms = rootGameObjects[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    GameObject gameObject = transforms[j].gameObject;
                    MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
                    for (int k = 0; k < components.Length; k++)
                    {
                        MonoBehaviour component = components[k];
                        if (component is IExcludeFromPlayerBuild)
                        {
                            buildExcludedComponents.Add(component);
                        }
                    }

                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject) > 0)
                    {
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                    }
                }
            }

            buildExcludedComponents.Sort((left, right) =>
                GetRequireComponentCount(right).CompareTo(GetRequireComponentCount(left)));

            for (int i = 0; i < buildExcludedComponents.Count; i++)
            {
                MonoBehaviour buildExcludedComponent = buildExcludedComponents[i];
                if (buildExcludedComponent == null)
                {
                    continue;
                }

                Object.DestroyImmediate(buildExcludedComponent);
            }
        }

        private static int GetRequireComponentCount(MonoBehaviour component)
            => component.GetType().GetCustomAttributes(typeof(RequireComponent), true).Length;
    }
}
