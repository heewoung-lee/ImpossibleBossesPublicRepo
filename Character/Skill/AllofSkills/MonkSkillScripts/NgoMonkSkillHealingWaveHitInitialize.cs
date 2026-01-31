using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Monk;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.MonkSkillScripts
{
    public class NgoMonkSkillHealingWaveHitInitialize : NgoPoolingInitializeBase
    {
        public class NgoMonkSkillHealingWaveHitFactory : NgoZenjectFactory<NgoMonkSkillHealingWaveHitInitialize>,
            IMonkFactoryMarker
        {
            [Inject]
            public NgoMonkSkillHealingWaveHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/MonkSkillPrefab/HealingWaveHit");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/MonkSkillPrefab/HealingWaveHit";
        public override int PoolingCapacity => 5;
    }
}
