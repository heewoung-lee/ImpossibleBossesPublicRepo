using CustomEditor;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Scene.GamePlayScene;
using Scene.GamePlayScene.Spawner;
using Zenject;

namespace Scene.BattleScene.Installer
{
    public class BattleSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TestSceneEditor>().FromComponentInHierarchy().AsSingle();//테스트 에디터 바인드

            //BattleScene 컴포넌트를 직접 바인드
            Container.BindInterfacesAndSelfTo<BattleScene>()
                .FromResolveGetter<TestSceneEditor, BattleScene>(t => t.GetComponent<BattleScene>())
                .AsSingle();

            //BaseScene 바인드
            Container.Bind<BaseScene>()
                .FromResolveGetter<TestSceneEditor>(t => t.GetComponent<BaseScene>())
                .AsSingle();
            
            Container.Bind<ISceneProvider>().To<SceneModeProvider>().AsSingle(); //현재 모드에 맞는 공급자 생성
            
            Container.Bind<ISceneStarter>().To<BattleSceneStarter>().AsSingle();
            Container.Bind<ISceneMover>().To<GamePlaySceneMover>().AsSingle();
            
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.NormalBoot).To<EmptySceneOnline>().AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.LocalTest).To<EmptySceneOnline>().AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.MultiTest_Solo).To<SceneConnectOnlineSolo>()
                .AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.MultiTest_Multi).To<SceneConnectOnlineMulti>()
                .AsCached();
            
                 
            //ISceneProvider가 현재 모드에 맞는 ISceneConnectOnline를 ID없이 소비자가 쓸 수 있게 자동으로 바인드
            Container.Bind<ISceneConnectOnline>().FromMethod(contextlevel =>
            {
                SceneMode sceneMode = contextlevel.Container.Resolve<ISceneProvider>().CurrentSceneMode;
                return contextlevel.Container.ResolveId<ISceneConnectOnline>(sceneMode);
            }); //소비자용 바인드 즉 프로바이더가 현재 모드를 판단해서 소비자가 쓸 바인드객체를 골라서 바인드함.



            
            
            // ISceneSpawnBehaviour(모드별로 전부 바인드)
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.NormalBoot).To<UnitBattleScene>().AsCached();
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.LocalTest).To<UnitBattleScene>()
                .AsCached();
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.MultiTest_Solo)
                .To<MockUnitNetworkBattleScene>().AsCached();
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.MultiTest_Multi)
                .To<MockUnitNetworkBattleScene>().AsCached();



            Container.Bind<ISceneSpawnBehaviour>().FromMethod(contextlevel =>
            {
                SceneMode sceneMode = contextlevel.Container.Resolve<ISceneProvider>().CurrentSceneMode;
                return contextlevel.Container.ResolveId<ISceneSpawnBehaviour>(sceneMode);
            });

            //TODO: 0824 배틀씬의 ISceneSpawnBehaviour 작성할것



        }
    }
}
