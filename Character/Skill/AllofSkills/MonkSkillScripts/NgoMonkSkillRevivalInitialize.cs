using Character.Skill.AllofSkills.Mage;
using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule.Mage;
using Module.PlayerModule.PlayerClassModule.Monk;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.MonkSkillScripts
{
    public class NgoMonkSkillRevivalInitialize : NgoPoolingInitializeBase
    {
        private const string RevivalSoundCueId = "RevivalSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class NgoMonkSkillRevivalFactory : NgoZenjectFactory<NgoMonkSkillRevivalInitialize>,
            IMonkFactoryMarker
        {
            [Inject]
            public NgoMonkSkillRevivalFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/MonkSkillPrefab/Revival");
            }
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            transform.position = targetGo.transform.position + Vector3.up;
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
            _soundPlayerBinder.PlayDetached(RevivalSoundCueId);
        }
        public override string PoolingNgoPath => "Prefabs/Player/VFX/MonkSkillPrefab/Revival";
        public override int PoolingCapacity => 5;
    }
}
