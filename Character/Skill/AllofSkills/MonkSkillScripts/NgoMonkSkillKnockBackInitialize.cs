using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Monk;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.MonkSkillScripts
{
    public class NgoMonkSkillKnockBackInitialize : NgoPoolingInitializeBase
    {
        public class NgoMonkSKillKnockBackFactory : NgoZenjectFactory<NgoMonkSkillKnockBackInitialize>,IMonkFactoryMarker
        {
            [Inject]
            public NgoMonkSKillKnockBackFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/MonkSkillPrefab/KnockBack");
            }
        }


        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            
            transform.position = targetGo.transform.position + (Vector3.up * 0.5f);
            
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/MonkSkillPrefab/KnockBack";
        public override int PoolingCapacity => 5;
    }
}