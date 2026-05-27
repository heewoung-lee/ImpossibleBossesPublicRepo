using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.Mage
{
    public class NgoMageSkillFrostNovaInitialize : NgoPoolingInitializeBase
    {
        private const string FrostNovaSoundCueId = "ProstNovaSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class NgoFrostNovaSkillFactory : NgoZenjectFactory<NgoMageSkillFrostNovaInitialize>, IMageFactoryMarker
        {
            [Inject]
            public NgoFrostNovaSkillFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Mage/Skill/FrostNova");
            }
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);

            transform.position = targetGo.transform.position + (Vector3.up * 0.5f);
            _soundPlayerBinder.PlayDetached(FrostNovaSoundCueId);
        }


        public override string PoolingNgoPath => "Prefabs/Player/VFX/Mage/Skill/FrostNova";
        public override int PoolingCapacity => 5;
    }
}
