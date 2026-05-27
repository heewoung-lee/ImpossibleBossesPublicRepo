using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss.Darkwizard.Minion
{
    public class NgoMinionGolemInitialize : NgoPoolingInitializeBase
    {
        public class NgoMinionGolemFactory : NgoZenjectFactory<NgoMinionGolemInitialize>
        {
            public NgoMinionGolemFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Minion/MinionGolem");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Minion/MinionGolem";
        public override int PoolingCapacity => 5;
    }
}