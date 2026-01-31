using GameManagers.Interface.NGOPoolManager;
using GameManagers.NGOPool.Implementation;
using Zenject;

namespace GameManagers.GameObjectContextInstaller.NgoPoolManager
{
    public class NetworkObjectPoolInstaller : MonoInstaller
    {
        
        public override void InstallBindings()
        {
            Container.Bind<INetworkObjectGetter>().To<DynamicNetworkObjectGetter>().AsSingle();
        }
    }
}
