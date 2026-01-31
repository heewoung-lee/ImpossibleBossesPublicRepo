using Zenject;

namespace Module.PlayerModule.PlayerClassModule.Monk
{
    public interface IMonkFactoryMarker{ }
    
    public class MonkNeedObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind(x => x.AllInterfaces())
                .To(x => x.AllNonAbstractClasses().DerivingFrom<IMonkFactoryMarker>())
                .AsSingle();

        }
    }
}