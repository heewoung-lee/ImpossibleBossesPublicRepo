using NetWork.NGO.Scene_NGO;
using ScenesScripts.CommonInstaller;
using ScenesScripts.CommonInstaller.Interfaces;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace ScenesScripts.ThirdBossScene
{
    public class ThirdBossSceneEnvironmentPostTestInstaller : MonoInstaller, ITestPostInstaller
    {
        [SerializeField] private bool _isTest = false;

        public override void InstallBindings()
        {
#if UNITY_EDITOR

            if (_isTest == true)
            {
                Container.BindInterfacesAndSelfTo<UICreateItemAndGoldButton.UICreateItemAndGoldButtonFactory>()
                    .AsSingle();


                Container.Rebind<ISceneConnectOnline>().To<SceneConnectOnlineMultiDirect>().AsSingle();
            }
#endif
        }
    }
}
