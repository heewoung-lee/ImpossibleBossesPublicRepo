using CustomEditor;
using ScenesScripts.BattleScene;
using ScenesScripts.CommonInstaller;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.FirstBossScene;
using ScenesScripts.GamePlayScene.Spawner;
using UnityEngine;
using Zenject;

namespace ScenesScripts.SecondBossScene.TestInstaller
{
    public enum SecondBossMultiTestLoadingMode
    {
        WithoutLoading,
        WithLoading
    }

    public class SecondBossSceneTestInstaller : MonoInstaller, ITestPreInstaller
    {
        [SerializeField] private SecondBossMultiTestLoadingMode _multiTestLoadingMode =
            SecondBossMultiTestLoadingMode.WithLoading;

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

            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.NormalBoot).To<UnitSecondBossScene>().FromResolve();
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.LocalTest).To<UnitSecondBossScene>()
                .FromResolve();

            if (_multiTestLoadingMode == SecondBossMultiTestLoadingMode.WithLoading)
            {
                Container.BindInterfacesTo<MockSceneLoadingSync.MockSceneLoadingSyncFactory>().AsSingle();
            }

            var multiTestSpawnBehaviourType = _multiTestLoadingMode == SecondBossMultiTestLoadingMode.WithLoading
                ? typeof(MockUnitNetworkSecondBossSceneWithLoading)
                : typeof(MockUnitNetworkSecondBossScene);

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
