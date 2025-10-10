using Scene.CommonInstaller;
using UnityEngine;
using Zenject;

namespace NetWork.NGO.RPCCaller
{
    public class NgoRpcCallerInstaller: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindFactory<DiContainer,GameObject, NgoZenjectHandler,NgoZenjectHandler.NgoZenjectHandlerFactory>();
            
            Container.BindInterfacesAndSelfTo<NgoRPCCaller>()
                .FromComponentOnRoot()
                .AsSingle(); 
            
            Container.BindInterfacesTo<NgoRPCSpawnController.NgoRPCSpawnerFactory>().AsCached();
        }
    }
}
