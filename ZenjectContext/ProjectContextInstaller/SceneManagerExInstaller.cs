using GameManagers.SceneManagement;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    public class SceneManagerExInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<SceneManagerEx>().AsSingle();
        }
    }
}
