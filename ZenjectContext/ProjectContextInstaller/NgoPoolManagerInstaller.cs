using GameManagers;
using GameManagers.Interface.NGOPoolManager;
using GameManagers.Pool;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    public class NgoPoolManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NgoPoolManager>().AsSingle();
        }
    }
}
