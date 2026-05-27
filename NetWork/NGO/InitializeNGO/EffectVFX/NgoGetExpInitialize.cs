using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using NetWork.BaseNGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.InitializeNGO.EffectVFX
{
    public class NgoGetExpInitialize : NgoPoolingInitializeBase
    {
        private const float FollowYOffset = 0.6f;
        private const string GetExpSoundCueId = "GETExpSFX";

        public class NgoGetExpFactory : NgoZenjectFactory<NgoGetExpInitialize>
        {
            public NgoGetExpFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService)
                : base(container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Common/GetExp");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Common/GetExp";
        public override int PoolingCapacity => 15;

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            transform.position = targetGo.transform.position + Vector3.up * FollowYOffset;
            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder))
            {
                soundPlayerBinder.PlayDetached(GetExpSoundCueId);
            }
            _vfxManager.FollowParticleRoutine(targetGo.transform, gameObject);
        }
    }
}
