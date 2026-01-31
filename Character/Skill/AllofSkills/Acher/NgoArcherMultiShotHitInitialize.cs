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
    public class NgoArcherMultiShotHitInitialize: NgoPoolingInitializeBase
    {
        public class ArcherMultiShotHitFactory : NgoZenjectFactory<NgoArcherMultiShotHitInitialize>,IArcherFactoryMarker
        {
            [Inject]
            public ArcherMultiShotHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>(  "Prefabs/Player/VFX/Archer/Skill/MultiShot_hit");
            }
        }


        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/Skill/MultiShot_hit";
        public override int PoolingCapacity => 30;
    }
}