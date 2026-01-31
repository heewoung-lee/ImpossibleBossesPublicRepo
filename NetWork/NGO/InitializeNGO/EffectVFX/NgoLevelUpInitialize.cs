using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using NetWork.BaseNGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.InitializeNGO.EffectVFX
{
    public class NgoLevelUpInitialize : NgoPoolingInitializeBase
    {
        public class NgoLevelUpFactory : NgoZenjectFactory<NgoLevelUpInitialize>
        {
            public NgoLevelUpFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) 
                : base(container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Common/Level_up");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Common/Level_up";
        public override int PoolingCapacity => 5;
        
        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
        }
    }
}