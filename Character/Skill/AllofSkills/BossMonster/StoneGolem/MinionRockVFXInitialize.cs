using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.StoneGolem
{
    public class MinionRockVFXInitialize : NgoPoolingInitializeBase
    {
        public class MinionRockVFXFactory : NgoZenjectFactory<MinionRockVFXInitialize>
        {
            [Inject]
            public MinionRockVFXFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoMinionRockVFX");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoMinionRockVFX";
        public override int PoolingCapacity => 10;
    }
}
