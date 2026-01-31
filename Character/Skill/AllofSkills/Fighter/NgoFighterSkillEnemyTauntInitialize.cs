using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Fighter;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Skill.AllofSkills.Fighter
{
    public class NgoFighterSkillEnemyTauntInitialize : NgoPoolingInitializeBase
    {
        public class NgoFighterSkillEnemyTauntFactory : NgoZenjectFactory<NgoFighterSkillEnemyTauntInitialize>,IFighterFactoryMarker
        {
            [Inject]
            public NgoFighterSkillEnemyTauntFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Fighter/Skill/Taunt_Enemy");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Fighter/Skill/Taunt_Enemy";

        public override int PoolingCapacity => 5;
        
        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
        }
    }
}