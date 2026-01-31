using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Archer;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Attack.Archer
{
    public class NgoArcherAttackHitInitialize: NgoPoolingInitializeBase
    {
        public class ArcherAttackHitFactory : NgoZenjectFactory<NgoArcherAttackHitInitialize>,IArcherFactoryMarker
        {
            [Inject]
            public ArcherAttackHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/Archer/ArcherAttackHit");
            }
        }
        
        
        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/ArcherAttackHit";
        public override int PoolingCapacity => 5;
    }
}
