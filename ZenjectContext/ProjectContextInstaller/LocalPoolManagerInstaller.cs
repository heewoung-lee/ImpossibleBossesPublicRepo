using GameManagers;
using Zenject;

namespace ProjectContextInstaller
{
    public class LocalPoolManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LocalPoolManager>().AsSingle();
            Container.BindFactory<LocalPoolManager.Pool,LocalPoolManager.Pool.PoolFactory>().AsSingle();
        }
    }
}
