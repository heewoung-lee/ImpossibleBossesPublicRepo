using System;
using System.Collections.Generic;
using Controller;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork;
using NetWork.BaseNGO;
using NetWork.NGO;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.Mage
{
    public class NgoMageSkillMeteorInitialize : NgoPoolingInitializeBase
    {
        private int _totalDamage = 0;
        private BaseStats _caster;
        private IAttackRange _attackRange;
        private RelayManager _relayManager;


        [Inject]
        public void Construct(RelayManager relayManager)
        {
            _relayManager = relayManager;   
        }
        
        public class NgoMeteorSkillFactory : NgoZenjectFactory<NgoMageSkillMeteorInitialize>, IMageFactoryMarker
        {
            [Inject]
            public NgoMeteorSkillFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Mage/Skill/Meteor");
            }
        }


        public void HitMeteorImpact(Collider col)
        {
            col.GetComponent<IDamageable>().OnAttacked(_attackRange,_totalDamage);
        }
        
        

        public override void StartParticleOption(float duration,NetworkParams networkParams)
        {
            base.StartParticleOption(duration,networkParams);

            ulong networkObjID = networkParams.ArgUlong;
            float damageMultiple = networkParams.ArgFloat;
            
            if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue
                    (networkObjID, out NetworkObject networkObj))
            {
                _caster = networkObj.GetComponent<BaseStats>();
                _totalDamage = (int)(_caster.Attack * damageMultiple);
                _attackRange = _caster.GetComponent<IAttackRange>();
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Mage/Skill/Meteor";
        public override int PoolingCapacity => 5;
    }
}