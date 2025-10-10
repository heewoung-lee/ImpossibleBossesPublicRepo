using GameManagers;
using GameManagers.Interface.RelayManagerInterface;
using GameManagers.Interface.RelayManagerInterface.Implementation;
using NetWork.NGO;
using Scene.CommonInstaller;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    public class RelayManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RelayManager>().AsSingle();

            Container.BindInterfacesTo<NgoRPCCaller.NgoRPCCallerFactory>()
                .AsSingle();
        }
    }

    
}
