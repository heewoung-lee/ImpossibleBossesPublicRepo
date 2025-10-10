using GameManagers;
using Zenject;

namespace ProjectContextInstaller
{
    public class SceneDataSaveAndLoaderInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SceneDataSaveAndLoader>().AsSingle();
        }
    }
}
