using System.Collections;
using GameManagers.ResourcesExManagement;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork.BaseNGO;
using NetWork.NGO;
using Stats;
using Stats.BaseStats;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss.Darkwizard
{
    public class NgoDarkWizardSectorAttackHitInitialize : NgoPoolingInitializeBase
    {
        public class DarkWizardSectorAttackHitFactory : NgoZenjectFactory<NgoDarkWizardSectorAttackHitInitialize>
        {
            public DarkWizardSectorAttackHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardSectorAttackHit");
            }
        }

        public override string PoolingNgoPath =>
            "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardSectorAttackHit";
        public override int PoolingCapacity => 30;
    }
}