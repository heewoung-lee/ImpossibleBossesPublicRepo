using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Archer;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.Acher
{
    public class NgoArcherMultiShotMuzzleInitialize :NgoPoolingInitializeBase
    {
        private IResourcesServices _resourcesServices;

        [Inject]
        private void Construct(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }
        
        public class ArcherMultiShotMuzzleFactory : NgoZenjectFactory<NgoArcherMultiShotMuzzleInitialize>,IArcherFactoryMarker
        {
            [Inject]
            public ArcherMultiShotMuzzleFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/Archer/Skill/MultiShot_muzzle");
            }
        }
        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            transform.position = targetGo.transform.position 
                                 + (targetGo.transform.forward * 0.5f)
                                 + (targetGo.transform.up * 0.5f);
            
            gameObject.transform.rotation = targetGo.transform.rotation;
            _resourcesServices.DestroyObject(gameObject,1f);
        }
        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/Skill/MultiShot_muzzle";
        public override int PoolingCapacity => 5;
    }
}