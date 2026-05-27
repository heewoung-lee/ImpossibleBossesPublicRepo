using GameManagers.NGOPoolManagement;
using GameManagers.NGOPoolManagement.Implementation;
using Zenject;

namespace GameManagers.GameObjectContextInstaller.NgoPoolManagement
{
    public class NetworkObjectPoolInstaller : MonoInstaller
    {
        
        public override void InstallBindings()
        {
            Container.Bind<INetworkObjectGetter>().To<DynamicNetworkObjectGetter>().AsSingle();
            
            Container.Bind<INetworkObjectPoolExpansionStrategy>()
                .To<ThresholdNetworkObjectPoolExpansionStrategy>()
                .AsSingle();
            
        }
    }
}
