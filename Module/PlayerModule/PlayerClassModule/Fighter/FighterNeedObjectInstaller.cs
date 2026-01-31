using Skill.AllofSkills.Fighter;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule.Fighter
{
    public interface IFighterFactoryMarker{}
    
    public class FighterNeedObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
           
            Container.Bind(x => x.AllInterfaces())
                .To(x => x.AllNonAbstractClasses().DerivingFrom<IFighterFactoryMarker>())
                .AsSingle();
        }
      
    }
}
