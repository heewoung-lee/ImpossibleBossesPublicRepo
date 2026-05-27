using System;
using GameManagers;
using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using NetWork;
using NetWork.NGO.Interface;
using Stats.BaseStats;
using UnityEngine;
using Util;
using VFX;
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
            switch (projector)
            {
                case IndicatorController indicatorController:
                    indicatorController.SetSpawnerBossNetworkObjectId(spawnparam.ArgUlong);
                    break;
                case NgoIndicatorController ngoIndicatorController:
                    ngoIndicatorController.SetSpawnerBossNetworkObjectId(spawnparam.ArgUlong);
                    break;
                case NgoArrowIndicatorController ngoArrowIndicatorController:
                    ngoArrowIndicatorController.SetSpawnerBossNetworkObjectId(spawnparam.ArgUlong);
                    break;
            }
            bool shouldAttackOnDone = spawnparam.ArgBoolean;
            float durationTime = spawnparam.ArgFloat;
            int attackDamage = spawnparam.ArgInt;
            IAttackRange attacker = GetComponent<IAttackRange>();
            projector.SetValue(Skill1Radius, Skill1Arc, spawnparam.ArgPosVector3, durationTime, Attack);
            void Attack()
            {
                if (shouldAttackOnDone == false)
                {
                    return;
                }

                TargetInSight.AttackTargetInCircle(attacker, projector.Radius, attackDamage);
            }
        }
    }
}
