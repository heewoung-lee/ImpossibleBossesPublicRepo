using Zenject;

namespace Module.PlayerModule.PlayerClassModule.Necromancer
{
    public interface INecromancerFactoryMarker{}
    public class NecromancerNeedObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind(x => x.AllInterfaces())
                .To(x => x.AllNonAbstractClasses().DerivingFrom<INecromancerFactoryMarker>())
                .AsSingle();
        }
    }
}
