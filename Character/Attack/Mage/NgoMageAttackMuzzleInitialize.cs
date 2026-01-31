using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Archer;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Attack.Mage
{
    public class NgoMageAttackMuzzleInitialize :NgoPoolingInitializeBase
    {
        public class MageAttackMuzzleFactory : NgoZenjectFactory<NgoMageAttackMuzzleInitialize>,IMageFactoryMarker
        {
            [Inject]
            public MageAttackMuzzleFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/Mage/MageAttackMuzzle");
            }
        }
        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            transform.position = targetGo.transform.position 
                                 + (targetGo.transform.forward * 0.3f)
                                 + (targetGo.transform.up * 0.3f);
            
            gameObject.transform.rotation = targetGo.transform.rotation;
        }
        public override string PoolingNgoPath => "Prefabs/Player/VFX/Mage/MageAttackMuzzle";
        public override int PoolingCapacity => 5;
    }
}