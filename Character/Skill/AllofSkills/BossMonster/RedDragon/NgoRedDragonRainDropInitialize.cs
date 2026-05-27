using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class NgoRedDragonRainDropInitialize : NgoPoolingInitializeBase
    {
        private const string RainDropCueId = "RedDragonRainDropSFX";

        public class RedDragonRainDropFactory : NgoZenjectFactory<NgoRedDragonRainDropInitialize>
        {
            [Inject]
            public RedDragonRainDropFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/RedDragon/DragonRainDropVFX");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/RedDragon/DragonRainDropVFX";
        public override int PoolingCapacity => 30;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder))
            {
                soundPlayerBinder.PlayDetached(RainDropCueId);
            }
        }
    }
}
