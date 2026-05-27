using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using Stats.MonsterStats;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss.Darkwizard.Minion.MinionGolem
{
    public class NgoMinionGolemAttackHitInitialize : NgoPoolingInitializeBase
    {
        private NgoMinionGolemAttackBehaviour _attackBehaviour;
        private MinionGolemStats _attacker;

        public class NgoMinionGolemAttackHitFactory : NgoZenjectFactory<NgoMinionGolemAttackHitInitialize>
        {
            public NgoMinionGolemAttackHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Minion/MinionGolemAttackHit");
            }
        }
        public override string PoolingNgoPath => "Prefabs/Enemy/Minion/MinionGolemAttackHit";
        public override int PoolingCapacity => 50;
    }
}
