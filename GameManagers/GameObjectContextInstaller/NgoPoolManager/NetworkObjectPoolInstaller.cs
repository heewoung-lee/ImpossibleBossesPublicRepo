using GameManagers.Interface.NGOPoolManager;
using GameManagers.Interface.NGOPoolManager.Implementation;
using Zenject;

namespace GameManagers.GameObjectContextInstaller.NgoPoolManager
{
    public class NetworkObjectPoolInstaller : MonoInstaller
    {
        
        public override void InstallBindings()
        {
            Container.Bind<INgoPoolRegister>().To<MockRegister>().AsSingle();

            Container.Bind<INetworkObjectGetter>().To<DynamicNetworkObjectGetter>().AsSingle();
        }
    }
}
