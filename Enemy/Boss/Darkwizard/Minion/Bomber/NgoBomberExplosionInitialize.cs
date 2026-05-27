using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using NetWork;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss.Darkwizard.Minion.Bomber
{
    public class NgoBomberExplosionInitialize : NgoPoolingInitializeBase
    {
        private const string BomberExplosionCueId = "MinionBomberExplosionSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class NgoBomberExplosionFactory : NgoZenjectFactory<NgoBomberExplosionInitialize>
        {
            public NgoBomberExplosionFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Minion/BomberExplosion");
            }
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public override void StartParticleOption(float duration, NetworkParams networkParams)
        {
            base.StartParticleOption(duration, networkParams);
            _soundPlayerBinder.PlayDetached(BomberExplosionCueId);
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Minion/BomberExplosion";
        public override int PoolingCapacity => 5;
    }
}
