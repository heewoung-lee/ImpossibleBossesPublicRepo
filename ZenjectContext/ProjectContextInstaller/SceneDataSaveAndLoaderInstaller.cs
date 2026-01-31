using GameManagers.Scene;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    public class SceneDataSaveAndLoaderInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SceneDataSaveAndLoader>().AsSingle();
            
            
        }
    }
}
