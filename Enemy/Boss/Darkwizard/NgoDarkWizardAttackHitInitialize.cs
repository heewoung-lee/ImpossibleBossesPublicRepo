using System.Collections;
using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork;
using NetWork.BaseNGO;
using NetWork.NGO;
using Stats;
using Stats.BaseStats;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss.Darkwizard
{
    public class NgoDarkWizardAttackHitInitialize : NgoPoolingInitializeBase
    {
        private const string DarkWizardAttackHitCueId = "DarkWizardAttackHitSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class DarkAttackHitFactory : NgoZenjectFactory<NgoDarkWizardAttackHitInitialize>
        {
            public DarkAttackHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardAttackHit");
            }
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public override void StartParticleOption(float duration, NetworkParams networkParams)
        {
            base.StartParticleOption(duration, networkParams);
            _soundPlayerBinder.PlayDetached(DarkWizardAttackHitCueId);
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardAttackHit";
        public override int PoolingCapacity => 5;
    }
}
