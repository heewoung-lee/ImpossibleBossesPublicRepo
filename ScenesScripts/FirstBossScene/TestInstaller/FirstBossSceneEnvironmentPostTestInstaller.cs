using NetWork.NGO.Scene_NGO;
using ScenesScripts.CommonInstaller;
using ScenesScripts.CommonInstaller.Interfaces;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace ScenesScripts.FirstBossScene.TestInstaller
{
    public class FirstBossSceneEnvironmentPostTestInstaller : MonoInstaller, ITestPostInstaller
    {
        [SerializeField] private bool _isTest = false;

        public override void InstallBindings()
        {
#if UNITY_EDITOR

            if (_isTest == true)
            {
                Container.BindInterfacesAndSelfTo<UICreateItemAndGoldButton.UICreateItemAndGoldButtonFactory>()
                    .AsSingle();

                Container.Rebind<TimeValue>().FromInstance(new TimeValue(300f, 10f, 7f)).AsCached();

                Container.Rebind<ISceneConnectOnline>().To<SceneConnectOnlineMultiDirect>().AsSingle();
            }
#endif
        }
    }
}