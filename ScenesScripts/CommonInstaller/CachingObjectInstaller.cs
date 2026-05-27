using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using Zenject;

namespace ScenesScripts.CommonInstaller
{
    public class CachingObjectInstaller : Installer<CachingObjectInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CachingObjectDictManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UICachingService>().AsSingle().NonLazy();
        }
    }
}