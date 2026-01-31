using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Necromancer;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.NecromancerSkillScripts
{
    public class NgoNecromancerSkillAmbush : NgoPoolingInitializeBase
    {
        public class NecromancerAmbushFactory : NgoZenjectFactory<NgoNecromancerSkillAmbush>,INecromancerFactoryMarker
        {
            [Inject]
            public NecromancerAmbushFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/NecromancerSkillPrefab/Ambush");
            }
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            
            transform.position = targetGo.transform.position + (Vector3.up * 0.5f);
            
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/NecromancerSkillPrefab/Ambush";
        public override int PoolingCapacity => 5;
    }
}