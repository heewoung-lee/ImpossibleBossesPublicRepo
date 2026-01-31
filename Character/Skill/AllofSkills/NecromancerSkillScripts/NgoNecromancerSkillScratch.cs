using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Necromancer;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.NecromancerSkillScripts
{
    public class NgoNecromancerSkillScratch : NgoPoolingInitializeBase
    {
        public class NecromancerScratchFactory : NgoZenjectFactory<NgoNecromancerSkillScratch>, INecromancerFactoryMarker
        {
            [Inject]
            public NecromancerScratchFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/NecromancerSkillPrefab/Scratch");
            }
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            gameObject.transform.rotation = targetGo.transform.rotation;
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/NecromancerSkillPrefab/Scratch";
        public override int PoolingCapacity => 5;
    }
}