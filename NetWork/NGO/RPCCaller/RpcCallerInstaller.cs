using NetWork.NGO.RPCCaller.Dependency;
using Zenject;

namespace NetWork.NGO.RPCCaller
{
    public class RpcCallerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ISpawnRpccallers>().To().AsSingle();   
        }
    }
}
