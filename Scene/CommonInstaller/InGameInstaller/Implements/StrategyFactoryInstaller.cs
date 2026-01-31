using DataType;
using DataType.Skill.Factory;
using Zenject;

namespace Scene.CommonInstaller.InGameInstaller.Implements
{
    public class StrategyFactoryInstaller : Installer<StrategyFactoryInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<StrategyFactory>().AsSingle();

            Container.Bind(x => x.AllInterfaces())
                .To(x => x.AllNonAbstractClasses().DerivingFrom<IStrategy>()).AsSingle();
        }
    }
}
