using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork;
using NetWork.BaseNGO;
using NetWork.NGO;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.Mage
{
    public class NgoMageSkillMeteorInitialize : NgoPoolingInitializeBase
    {
        private const string MeteorSoundCueId = "MeteorSFX";

        private int _totalDamage = 0;
        private BaseStats _caster;
        private IAttackRange _attackRange;
        private RelayManager _relayManager;
        private SoundPlayerBinder _soundPlayerBinder;

        [SerializeField, Range(0f, 1f)]
        private float _cameraShakeIntensity = 0.6f;

        [SerializeField, Min(0f)]
        private float _cameraShakeDuration = 0.25f;


        [Inject]
        public void Construct(RelayManager relayManager)
        {
            _relayManager = relayManager;   
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }
        
        public class NgoMeteorSkillFactory : NgoZenjectFactory<NgoMageSkillMeteorInitialize>, IMageFactoryMarker
        {
            [Inject]
            public NgoMeteorSkillFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Mage/Skill/Meteor");
            }
        }


        public void HitMeteorImpact(Collider col)
        {
            col.GetComponent<IDamageable>().OnAttacked(_attackRange,_totalDamage);
        }

        public void RequestMeteorImpactCameraShake()
        {
            if (_relayManager.NetworkManagerEx.IsHost)
            {
                _relayManager.NgoRPCCaller.RequestCameraShakeRpc(_cameraShakeIntensity, _cameraShakeDuration);
            }
        }
        
        

        public override void StartParticleOption(float duration,NetworkParams networkParams)
        {
            base.StartParticleOption(duration,networkParams);
            _soundPlayerBinder.PlayDetached(MeteorSoundCueId);

            ulong networkObjID = networkParams.ArgUlong;
            float damageMultiple = networkParams.ArgFloat;
            
            if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue
                    (networkObjID, out NetworkObject networkObj))
            {
                _caster = networkObj.GetComponent<BaseStats>();
                _totalDamage = (int)(_caster.Attack * damageMultiple);
                _attackRange = _caster.GetComponent<IAttackRange>();
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Mage/Skill/Meteor";
        public override int PoolingCapacity => 5;
    }
}
