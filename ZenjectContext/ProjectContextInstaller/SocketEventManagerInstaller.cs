using GameManagers;
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
