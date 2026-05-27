using CustomEditor;
using ScenesScripts.BattleScene;
using ScenesScripts.CommonInstaller;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.FirstBossScene;
using ScenesScripts.GamePlayScene.Spawner;
using UnityEngine;
using Zenject;

namespace ScenesScripts.FirstBossScene.Installer
{
    public interface IMockSceneLoadingSyncFactory
    {
        MockSceneLoadingSync Create(int expectedPlayerCount);
    }

    public enum MultiTestLoadingMode
    {
        WithoutLoading,
        WithLoading
    }

    public class FirstBossSceneTestInstaller : MonoInstaller, ITestPreInstaller
    {
        [SerializeField] private MultiTestLoadingMode _multiTestLoadingMode = MultiTestLoadingMode.WithLoading;

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

            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.NormalBoot).To<UnitFirstBossScene>().FromResolve();
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.LocalTest).To<UnitFirstBossScene>()
                .FromResolve();

            if (_multiTestLoadingMode == MultiTestLoadingMode.WithLoading)
            {
                // 4.8 추가:
                // 모든 씬 로딩이 완료될 때 각 클라이언트의 완료 상태를 파악해야 해서
                // 로딩 sync mock도 함께 바인드한다.
                Container.BindInterfacesTo<MockSceneLoadingSync.MockSceneLoadingSyncFactory>().AsSingle();
            }

            // 4.8 추가:
            // 멀티 테스트에서 로딩 연출 포함 여부를 인스펙터 enum으로 분기해서
            // 로딩 있는 mock / 로딩 없는 mock 중 하나를 바인드한다.
            var multiTestSpawnBehaviourType = _multiTestLoadingMode == MultiTestLoadingMode.WithLoading
                ? typeof(MockUnitNetworkFirstBossSceneWithLoading)
                : typeof(MockUnitNetworkFirstBossScene);

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
