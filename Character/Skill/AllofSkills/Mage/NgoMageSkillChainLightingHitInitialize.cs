using System;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.Mage
{
    public class NgoMageSkillChainLightingHitInitialize : NgoPoolingInitializeBase
    {
        public class NgoChainLightingHitFactory : NgoZenjectFactory<NgoMageSkillChainLightingHitInitialize>, IMageFactoryMarker
        {
            [Inject]
            public NgoChainLightingHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Mage/Skill/ChainLightingHit");
            }
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            transform.position = targetGo.transform.position + (Vector3.up * 0.5f);
        }


        public override string PoolingNgoPath => "Prefabs/Player/VFX/Mage/Skill/ChainLightingHit";
        public override int PoolingCapacity => 5;
    }
}