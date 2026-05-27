using Zenject;

namespace ScenesScripts.CommonInstaller.OutGameInstaller
{
    public class CommonOutGameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            DefaultObjectCreatorInstaller.Install(Container);
            CachingObjectInstaller.Install(Container);
        }
    }
}
