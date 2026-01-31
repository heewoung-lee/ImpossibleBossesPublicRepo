using Zenject;

namespace Module.PlayerModule.PlayerClassModule.Mage
{
    public interface IMageFactoryMarker{}
    
    public class MageNeedObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind(x => x.AllInterfaces())
                .To(x => x.AllNonAbstractClasses().DerivingFrom<IMageFactoryMarker>())
                .AsSingle();
        }

    }
}
