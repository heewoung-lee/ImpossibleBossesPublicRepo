using Character.Skill.AllofSkills.BossMonster.StoneGolem;
using NetWork.BossGolem_NGO;
using NetWork.NGO;
using NetWork.NGO.Scene_NGO;
using Skill.AllofSkills.BossMonster.StoneGolem;
using UnityEngine;
using VFX;
using Zenject;

namespace Enemy.Boss.Installer
{
    public class BossGolemCreateObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            
            Container.BindInterfacesTo<DustParticleInitialize.DustParticleFactory>().AsSingle();
            
            Container.BindInterfacesTo<DustParticleInitialize.BiGDustParticleFactory>().AsSingle();
            
            Container.BindInterfacesTo<StoneGolemSkill1IndicatorInitialize.StoneGolemSkill1IndicatorFactory>()
                .AsSingle();

            Container
                .BindInterfacesTo<BossAttackIndicatorInitialize.BossAttackIndicatorFactory>()
                .AsSingle();

            Container
                .BindInterfacesTo<StoneGolemRollingRockInitialize.StoneGolemRollingRockFactory>()
                .AsSingle();

            Container
                .BindInterfacesTo<MinionRockVFXInitialize.MinionRockVFXFactory>()
                .AsSingle();

            Container
                .BindInterfacesTo<NgoBossSkill1AttackInitialize.NgoBossSkill1AttackFactory>()
                .AsSingle();

            Container
                .BindInterfacesTo<NgoBossSkill1AttackHitInitialize.NgoBossSkill1AttackHitFactory>()
                .AsSingle();

            Container
                .BindInterfacesTo<NgoRockToThrowSkyInitialize.NgoRockToThrowSkyFactory>()
                .AsSingle();

            
            Container.BindInterfacesTo<DropItemBehaviour.DropItemBehaviourFactory>().AsSingle();
        }
    }
}
