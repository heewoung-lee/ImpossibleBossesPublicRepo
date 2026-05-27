using GameManagers;
using GameManagers.SocketManagement;
using Zenject;

namespace ProjectContextInstaller
{
    public class SocketEventManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SocketEventManager>().AsSingle();
        }
    }
}
