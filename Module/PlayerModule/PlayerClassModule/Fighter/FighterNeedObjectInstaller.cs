using Skill.AllofSkills.Fighter;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule.Fighter
{
    public class FighterNeedObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NgoFighterSkillSlashInitialize.NgoFighterSkillSlashFactory>().AsSingle();

            Container.BindInterfacesTo<NgoFighterSkillDeterminationInitialize.NgoFighterSkillDeterminationFactory>().AsSingle();

            Container.BindInterfacesTo<NgoFighterSkillTauntInitialize.NgoFighterSkillTauntFactory>().AsSingle();
            
            Container.BindInterfacesTo<NgoFighterSkillRoarInitialize.NgoFighterSkillRoarFactory>().AsSingle();
            
            Container.BindInterfacesTo<NgoFighterSkillEnemyTauntInitialize.NgoFighterSkillEnemyTauntFactory>().AsSingle();

        }
      
    }
}
