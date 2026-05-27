using Character.Skill.AllofSkills.BossMonster.RedDragon;
using NetWork.BossGolem_NGO;
using VFX;
using Zenject;

namespace Enemy.Boss.Installer
{
    public class BossRedDragonObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NgoRedDragonAttackInitialize.RedDragonAttackFactory>().AsSingle();
            Container.BindInterfacesTo<NgoRedDragonBreathInitialize.RedDragonBreathFactory>().AsSingle();
            Container.BindInterfacesTo<NgoRedDragonProjectileInitialize.RedDragonProjectileFactory>().AsSingle();
            Container.BindInterfacesTo<RedDragonProjectileHitVFXInitialize.RedDragonProjectileHitVFXFactory>().AsSingle();
            Container.BindInterfacesTo<NgoRedDragonRainDropInitialize.RedDragonRainDropFactory>().AsSingle();
            Container.BindInterfacesTo<NgoRedDragonRootDebuffInitialize.RedDragonRootDebuffFactory>().AsSingle();
            Container.BindInterfacesTo<NgoRootBinderInitialize.RootBinderFactory>().AsSingle();
            Container.BindInterfacesTo<RedDragonRainIndicatorInitialize.RedDragonRainIndicatorFactory>().AsSingle();
            Container.BindInterfacesTo<RedDragonLandingVfxInitialize.RedDragonLandingVfxFactory>().AsSingle();
            Container.BindInterfacesTo<RedDragonLandingIndicatorInitialize.RedDragonLandingIndicatorFactory>().AsSingle();
            Container.BindInterfacesTo<RedDragonArrowIndicatorPoolingInitialize.RedDragonArrowIndicatorPoolingFactory>().AsSingle();
            Container.BindInterfacesTo<RedDragonBreathIndicatorInitialize.RedDragonBreathIndicatorFactory>().AsSingle();
            Container.BindInterfacesTo<RedDragonSpawnIndicatorInitialize.RedDragonSpawnIndicatorFactory>().AsSingle();
            Container.BindInterfacesTo<RedDragonAttackIndicatorInitialize.RedDragonAttackIndicatorFactory>().AsSingle();
            Container.BindInterfacesTo<DropItemBehaviour.DropItemBehaviourFactory>().AsSingle();
        }
    }
}
