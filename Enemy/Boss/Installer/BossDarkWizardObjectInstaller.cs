using Enemy.Boss.Darkwizard;
using Enemy.Boss.Darkwizard.Minion;
using Enemy.Boss.Darkwizard.Minion.Bomber;
using Enemy.Boss.Darkwizard.Minion.MinionGolem;
using NetWork.BossGolem_NGO;
using Skill.AllofSkills.BossMonster.StoneGolem;
using VFX;
using Zenject;

namespace Enemy.Boss.Installer
{
    public class BossDarkWizardObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NgoDarkWizardSectorAttackInitialize.DarkWizardSectorAttackFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<NgoDarkWizardSectorAttackHitInitialize.DarkWizardSectorAttackHitFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<NgoDarkWizardSectorAttackMuzzleInitialize.NgoDarkWizardSectorAttackMuzzleFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<NgoMinionSpawnImpactInitialize.NgoMinionSpawnImpactFactory>().AsSingle();
                
            Container.BindInterfacesAndSelfTo<NgoMinionGolemAttackInitialize.NgoMinionGolemAttackFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<NgoMinionGolemAttackHitInitialize.NgoMinionGolemAttackHitFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<NgoDarkWizardSpawnIndicatorInitialize.NgoDarkWizardSpawnIndicatorFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<NgoBomberInitialize.NgoBomberFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<NgoMinionGolemInitialize.NgoMinionGolemFactory>().AsSingle();

            Container.BindInterfacesAndSelfTo<NgoDarkWizardAttackInitialize.DarkAttackFactory>().AsSingle();

            Container.BindInterfacesAndSelfTo<NgoDarkWizardAttackHitInitialize.DarkAttackHitFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<NgoDarkWizardAttackMuzzleInitialize.DarkAttackMuzzleFactory>().AsSingle();
            
            Container.BindInterfacesTo<DropItemBehaviour.DropItemBehaviourFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<NgoBomberExplosionInitialize.NgoBomberExplosionFactory>().AsSingle();
            
        }
    }
}