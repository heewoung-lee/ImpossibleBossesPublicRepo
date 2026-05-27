using GameManagers;
using GameManagers.NGOPoolManagement;
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
