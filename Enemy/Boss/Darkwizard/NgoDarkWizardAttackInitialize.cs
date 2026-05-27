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
    public class NgoDarkWizardAttackInitialize : NgoPoolingInitializeBase
    {
        public class DarkAttackFactory : NgoZenjectFactory<NgoDarkWizardAttackInitialize>
        {
            public DarkAttackFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardAttack");
            }
        }
        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardAttack";
        public override int PoolingCapacity => 5;
    }
}