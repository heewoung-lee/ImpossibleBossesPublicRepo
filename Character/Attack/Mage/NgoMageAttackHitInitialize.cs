using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Attack.Mage
{
    public class NgoMageAttackHitInitialize: NgoPoolingInitializeBase
    {
        public class MageAttackHitFactory : NgoZenjectFactory<NgoMageAttackHitInitialize>,IMageFactoryMarker
        {
            [Inject]
            public MageAttackHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/Mage/MageAttackHit");
            }
        }
        public override string PoolingNgoPath => "Prefabs/Player/VFX/Mage/MageAttackHit";
        public override int PoolingCapacity => 5;
    }
}