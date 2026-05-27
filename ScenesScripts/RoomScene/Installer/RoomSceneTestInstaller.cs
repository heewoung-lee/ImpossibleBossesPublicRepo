using CustomEditor;
using ScenesScripts.CommonInstaller;
using ScenesScripts.CommonInstaller.Interfaces;
using Zenject;

namespace ScenesScripts.RoomScene.Installer
{
    public interface ITestNetworkConnect
    {
        public ISceneConnectOnline ConnectOnline { get; }
    }
    
    public class RoomSceneTestInstaller : MonoInstaller,ITestPreInstaller
    {
        public override void InstallBindings()
        {
#if  UNITY_EDITOR
            Container.BindInterfacesAndSelfTo<TestSceneEditor>().FromComponentInHierarchy().AsSingle();
            
            Container.Bind<ISceneProvider>().To<SceneModeProvider>().AsSingle();


            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.NormalBoot).To<EmptySceneOnline>().AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.MultiTest_Solo).To<SceneConnectOnlineSolo>().AsCached();
            Container.Bind<ISceneConnectOnline>().WithId(SceneMode.MultiTest_Multi).To<SceneConnectOnlineMulti>().AsCached();
         
            Container.Rebind<ISceneConnectOnline>().FromMethod(contextlevel =>
            {
                SceneMode sceneMode = contextlevel.Container.Resolve<ISceneProvider>().CurrentSceneMode;
                return contextlevel.Container.ResolveId<ISceneConnectOnline>(sceneMode);
            }); //소비자용 바인드 즉 프로바이더가 현재 모드를 판단해서 소비자가 쓸 바인드객체를 골라서 바인드함.
#endif
        }
    }
}


