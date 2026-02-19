using System;
using CustomEditor;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Scene.GamePlayScene.Spawner;
using Scene.GamePlayScene.Spwaner;
using Zenject;


namespace Scene.GamePlayScene.Installer
{
    //2.12일 추가 해당 테스터는 네트워크연결을 빠르게 하기위해
    //네트워크 연결을 어떻게 할지 결정하는 연결자가 필요함.
    //프로덕션에서는 이전 단계에서 네트워크가 연결이 돼서 필요없지만
    //에디터상에 다른 씬을 직접 연결해서 쓸때는 해당 구현체가 필요
    public interface ITestNetworkConnect
    {
        public ISceneConnectOnline ConnectOnline { get; }
    }
    
    //2.12일 수정 테스트 바인더로 고정해 에디터상에 실행할땐 맨뒤에 실행해 바인드를 덮을 수 있게 설계
    //빌드시에는 원본만 바인드되고 이 로직은 바인드가 안되니 에디터와 프로덕션용을 혼용해서 하되 빌드시에는 에디터만 쏙빠져 빌드됨. 
    public class GamePlaySceneTestInstaller : MonoInstaller,ITestPreInstaller
    {
        
        public override void InstallBindings()
        {
            #if UNITY_EDITOR
            Container.BindInterfacesAndSelfTo<TestSceneEditor>().FromComponentInHierarchy().AsSingle(); //테스트 에디터 바인드

            Container.Bind<ISceneProvider>().To<SceneModeProvider>().AsSingle(); //현재 모드에 맞는 공급자 생성

            // ISceneConnectOnline(모드별로 전부 바인드)
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.NormalBoot).To<EmptySceneOnline>().AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.LocalTest).To<EmptySceneOnline>().AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.MultiTest_Solo).To<SceneConnectOnlineSolo>()
                .AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.MultiTest_Multi).To<SceneConnectOnlineMulti>()
                .AsCached();

            //ISceneProvider가 현재 모드에 맞는 ISceneConnectOnline를 ID없이 소비자가 쓸 수 있게 자동으로 바인드
            Container.Rebind<ISceneConnectOnline>().FromMethod(contextlevel =>
            {
                SceneMode sceneMode = contextlevel.Container.Resolve<ISceneProvider>().CurrentSceneMode;
                return contextlevel.Container.ResolveId<ISceneConnectOnline>(sceneMode);
            }); //소비자용 바인드 즉 프로바이더가 현재 모드를 판단해서 소비자가 쓸 바인드객체를 골라서 바인드함.


            // ISceneSpawnBehaviour(모드별로 전부 바인드)
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.NormalBoot).To<UnitNetGamePlayScene>()
                .FromResolve();//노멀모드는 이미 노멀부트에서 바인드 되었으므로 컨테이너에서 가져옴
            
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.LocalTest).To<MockPlaySceneLocalSpawnBehaviour>()
                .AsCached();


            Container.Bind(
                typeof(MockPlaySceneNetworkSpawnBehaviour), //본체를 바인드해야 FromResolve가능
                typeof(IInitializable),
                typeof(IDisposable)).To<MockPlaySceneNetworkSpawnBehaviour>().AsCached();

            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.MultiTest_Solo)
                .To<MockPlaySceneNetworkSpawnBehaviour>().FromResolve();
            Container.Bind<ISceneSpawnBehaviour>().WithId(SceneMode.MultiTest_Multi)
                .To<MockPlaySceneNetworkSpawnBehaviour>().FromResolve();


            //ISceneProvider가 현재 모드에 맞는 ISceneSpawnBehaviour ID없이 소비자가 쓸 수 있게 자동으로 바인드
            Container.Rebind<ISceneSpawnBehaviour>().FromMethod(contextlevel =>
            {
                SceneMode sceneMode = contextlevel.Container.Resolve<ISceneProvider>().CurrentSceneMode;
                return contextlevel.Container.ResolveId<ISceneSpawnBehaviour>(sceneMode);
            });
            #endif
        }
    }
}

