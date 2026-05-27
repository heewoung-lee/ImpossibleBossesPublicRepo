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
    public class NgoDarkWizardSectorAttackMuzzleInitialize : NgoPoolingInitializeBase
    {
        public class NgoDarkWizardSectorAttackMuzzleFactory : NgoZenjectFactory<NgoDarkWizardSectorAttackMuzzleInitialize>
        {
            public NgoDarkWizardSectorAttackMuzzleFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardSectorAttackMuzzle");
            }
        }

        public override string PoolingNgoPath =>
            "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardSectorAttackMuzzle";
        public override int PoolingCapacity => 5;
    }
}