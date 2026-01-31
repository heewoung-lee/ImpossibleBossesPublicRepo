using System;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using GameManagers.ResourcesEx;
using NetWork;
using NetWork.NGO.Interface;
using Scene.CommonInstaller.Interfaces;
using Stats.BaseStats;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Skill.AllofSkills.BossMonster.StoneGolem
{
    public class StoneGolemSkill1IndicatorInitialize : MonoBehaviour,ISpawnBehavior
    {
        private IVFXManagerServices _vfxManagerServices;

        [Inject]
        public void Construct(IVFXManagerServices vfxManagerServices)
        {
            _vfxManagerServices = vfxManagerServices;
        }
        
        public class StoneGolemSkill1IndicatorFactory : GameObjectContextFactory<StoneGolemSkill1IndicatorInitialize>
        {
            [Inject]
            public StoneGolemSkill1IndicatorFactory(DiContainer container, IResourcesServices loadService,
                IFactoryManager factoryManager) : base(container, factoryManager)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Indicator/Boss_Skill1_Indicator");
            }
            
        }
        
        
        private const float Skill1Radius = 2f;
        private const float Skill1Arc = 360f;
        public void SpawnObjectToLocal(in NetworkParams spawnparam, string runtimePath = null)
        {
            
            transform.SetParent(_vfxManagerServices.VFXRoot,false);
        
            IIndicatorBahaviour projector = GetComponent<IIndicatorBahaviour>();
            IAttackRange attacker = (projector as Component).GetComponent<IAttackRange>();
            int attackDamage = spawnparam.ArgInt;
            float durationTime = spawnparam.ArgFloat;
            projector.SetValue(Skill1Radius, Skill1Arc, spawnparam.ArgPosVector3, durationTime, Attack);
            void Attack()
            {
                TargetInSight.AttackTargetInCircle(attacker, projector.Radius, attackDamage);
            }
        }
    }
}
