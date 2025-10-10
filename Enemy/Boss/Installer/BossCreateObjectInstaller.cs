using NetWork.Boss_NGO;
using NetWork.NGO;
using NetWork.NGO.Scene_NGO;
using Skill.AllofSkills.BossMonster.StoneGolem;
using UnityEngine;
using VFX;
using Zenject;

namespace Enemy.Boss.Installer
{
    public class BossCreateObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            
            Container.BindInterfacesTo<DustParticleInitialize.DustParticleFactory>().AsSingle();
            
            Container.BindInterfacesTo<DustParticleInitialize.BiGDustParticleFactory>().AsSingle();
            
            Container.BindInterfacesTo<StoneGolemSkill1StoneInitialize.StoneGolemSkill1StoneFactory>().AsSingle();

            Container.BindInterfacesTo<StoneGolemSkill1IndicatorInitialize.StoneGolemSkill1IndicatorFactory>()
                .AsSingle();

            Container
                .BindInterfacesTo<StoneGolemAttackIndicatorPoolingInitialize.StoneGolemAttackIndicatorPoolingFactory>()
                .AsSingle();

            
            Container.BindInterfacesTo<DropItemBehaviour.DropItemBehaviourFactory>().AsSingle();
            
            Container.BindInterfacesTo<NgoMoveDownTownBehaviour.NgoMoveDownTownBehaviourFactory>().AsSingle();
            
        }
    }
}
