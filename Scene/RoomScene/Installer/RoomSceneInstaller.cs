using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Zenject;

namespace Scene.RoomScene.Installer
{
    public class RoomSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ISceneStarter>().To<RoomSceneStarter>().AsSingle();
            Container.Bind<ISceneConnectOnline>().To<EmptySceneOnline>().AsSingle();
        }
    }
}
