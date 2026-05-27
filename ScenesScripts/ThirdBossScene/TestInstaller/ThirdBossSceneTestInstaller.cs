using CustomEditor;
using ScenesScripts.BattleScene;
using ScenesScripts.CommonInstaller;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.FirstBossScene;
using ScenesScripts.GamePlayScene.Spawner;
using UnityEngine;
using Zenject;

namespace ScenesScripts.ThirdBossScene.TestInstaller
{
    public enum ThirdBossMultiTestLoadingMode
    {
        WithoutLoading,
        WithLoading
    }

    public class ThirdBossSceneTestInstaller : MonoInstaller, ITestPreInstaller
    {
        [SerializeField] private ThirdBossMultiTestLoadingMode _multiTestLoadingMode =
            ThirdBossMultiTestLoadingMode.WithLoading;

        public override void InstallBindings()
        {
#if UNITY_EDITOR
            Container.BindInterfacesAndSelfTo<TestSceneEditor>().FromComponentInHierarchy().AsSingle();

            Container.Bind<ISceneProvider>().To<SceneModeProvider>().AsSingle();

            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.NormalBoot).To<EmptySceneOnline>().AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.LocalTest).To<EmptySceneOnline>().AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.MultiTest_Solo).To<SceneConnectOnlineSolo>()
                .AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.MultiTest_Multi).To<SceneConnectOnlineMulti>()
                .AsCached();

            Container.Bind<ISceneConnectOnline>().FromMethod(contextlevel =>
            {
                SceneMode sceneMode = contextlevel.Container.Resolve<ISceneProvider>().CurrentSceneMode;
                return contextlevel.Container.ResolveId<ISceneConnectOnline>(sceneMode);
            });

            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.NormalBoot).To<UnitThirdBossScene>().FromResolve();
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.LocalTest).To<UnitThirdBossScene>().FromResolve();

            if (_multiTestLoadingMode == ThirdBossMultiTestLoadingMode.WithLoading)
            {
                Container.BindInterfacesTo<MockSceneLoadingSync.MockSceneLoadingSyncFactory>().AsSingle();
            }

            var multiTestSpawnBehaviourType = _multiTestLoadingMode == ThirdBossMultiTestLoadingMode.WithLoading
                ? typeof(MockUnitNetworkThirdBossSceneWithLoading)
                : typeof(MockUnitNetworkThirdBossScene);

            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.MultiTest_Solo)
                .To(multiTestSpawnBehaviourType).AsCached();
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.MultiTest_Multi)
                .To(multiTestSpawnBehaviourType).AsCached();

            Container.Rebind<ISceneSpawnBehaviour>().FromMethod(contextlevel =>
            {
                SceneMode sceneMode = contextlevel.Container.Resolve<ISceneProvider>().CurrentSceneMode;
                return contextlevel.Container.ResolveId<ISceneSpawnBehaviour>(sceneMode);
            });
#endif
        }
    }
}
