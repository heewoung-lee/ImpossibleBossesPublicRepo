using GameManagers.RelayManagement;
using GameManagers.RelayManagement.Implementation;
using NetWork.NGO;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    public class RelayManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RelayManager>().AsSingle();

            Container.BindInterfacesAndSelfTo<RelayConnection>().AsSingle();
            
            Container.BindInterfacesTo<NgoRPCCaller.NgoRPCCallerFactory>()
                .AsSingle();
        }
    }

    
}
