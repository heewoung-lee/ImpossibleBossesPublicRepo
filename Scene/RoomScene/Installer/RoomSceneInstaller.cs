using GameManagers;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Zenject;

namespace Scene.RoomScene.Installer
{
   public class RoomSceneInstaller : MonoInstaller
   {
      public override void InstallBindings()
      {
         Container.BindInterfacesAndSelfTo<RoomPlayScene>().FromComponentInHierarchy().AsSingle();//테스트 에디터 바인드
         
         Container.Bind<BaseScene>()
            .FromResolveGetter<RoomPlayScene>(t => t.GetComponent<BaseScene>());
         
         Container.Bind<ISceneProvider>().To<SceneModeProvider>().AsSingle(); 
         
         Container.Bind<ISceneStarter>().To<RoomSceneStarter>().AsSingle();
         
         Container.Bind<ISceneConnectOnline>().WithId(SceneMode.NormalBoot).To<EmptySceneOnline>().AsSingle();
         Container.Bind<ISceneConnectOnline>().WithId(SceneMode.MultiTest_Solo).To<SceneConnectOnlineSolo>().AsSingle();
         Container.Bind<ISceneConnectOnline>().WithId(SceneMode.MultiTest_Multi).To<SceneConnectOnlineMulti>().AsSingle();
         
         Container.Bind<ISceneConnectOnline>().FromMethod(contextlevel =>
         {
            SceneMode sceneMode = contextlevel.Container.Resolve<ISceneProvider>().CurrentSceneMode;
            return contextlevel.Container.ResolveId<ISceneConnectOnline>(sceneMode);
         }); //소비자용 바인드 즉 프로바이더가 현재 모드를 판단해서 소비자가 쓸 바인드객체를 골라서 바인드함.
      }
   }
}
