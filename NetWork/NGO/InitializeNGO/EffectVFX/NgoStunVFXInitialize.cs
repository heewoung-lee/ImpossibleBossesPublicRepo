using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using NetWork.BaseNGO;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.InitializeNGO.EffectVFX
{
    public class NgoStunVFXInitialize : NgoPoolingInitializeBase
    {
        private const string StunSoundCueId = "StunSFX";

        public class NgoStunVFXFactory : NgoZenjectFactory<NgoStunVFXInitialize>
        {
            public NgoStunVFXFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService)
                : base(container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Common/NgoStunVFX");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Common/NgoStunVFX";
        public override int PoolingCapacity => 8;

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);

            HeadTr headTr = targetGo.GetComponentInChildren<HeadTr>(true);
            if (headTr == null)
            {
                throw new MissingComponentException(
                    $"[{nameof(NgoStunVFXInitialize)}] {targetGo.name} is missing {nameof(HeadTr)}.");
            }

            transform.position = headTr.transform.position;
            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder))
            {
                soundPlayerBinder.PlayDetached(StunSoundCueId);
            }
            _vfxManager.FollowParticleRoutine(headTr.transform, gameObject);
        }
    }
}
