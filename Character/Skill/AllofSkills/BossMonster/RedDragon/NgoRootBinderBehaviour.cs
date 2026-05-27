using Controller.CrowdControl;
using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using Stats;
using Stats.MonsterStats;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    [RequireComponent(typeof(Collider), typeof(RootBinderStats))]
    public class NgoRootBinderBehaviour : NetworkBehaviour
    {
        private const string RootDebuffVfxPath = "Prefabs/Enemy/Boss/AttackPattern/RedDragon/RootDebuffVFX";
        private const float RootDebuffVfxDuration = 3f;
        private const float DieDespawnNormalizedTime = 0.9f;
        private const float LifetimeSeconds = 5f;

        private static readonly int DieAnimHash = Animator.StringToHash("Die");

        private IResourcesServices _resourcesServices;
        private IVFXManagerServices _vfxManagerServices;

        private RootBinderStats _rootBinderStats;
        private Collider _triggerCollider;
        private Animator _animator;

        private bool _hasTriggered;
        private bool _isWaitingForDieAnimationEnd;
        private float _lifeEndTime;

        [Inject]
        public void Construct(IResourcesServices resourcesServices, IVFXManagerServices vfxManagerServices)
        {
            _resourcesServices = resourcesServices;
            _vfxManagerServices = vfxManagerServices;
        }

        private void Awake()
        {
            _rootBinderStats = GetComponent<RootBinderStats>();
            _triggerCollider = GetComponent<Collider>();
            _animator = GetComponentInChildren<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            ResetLocalState();
            _rootBinderStats.IsDeadValueChagneEvent += OnIsDeadValueChanged;

            if (IsHost)
            {
                _lifeEndTime = Time.time + LifetimeSeconds;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _rootBinderStats.IsDeadValueChagneEvent -= OnIsDeadValueChanged;

            if (IsHost)
            {
                _rootBinderStats.ResetForPoolDespawn();
            }

            ResetForPoolReuse();
        }

        private void Update()
        {
            UpdateLifetime();
            UpdateDead();
        }

        private void OnTriggerEnter(Collider other)
        {
            TryTriggerRootDebuff(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryTriggerRootDebuff(other);
        }

        private void OnIsDeadValueChanged(bool previousValue, bool newValue)
        {
            if (newValue == false)
            {
                return;
            }

            EnterDeadState();
        }

        private void ResetLocalState()
        {
            _hasTriggered = false;
            _isWaitingForDieAnimationEnd = false;
            _lifeEndTime = 0f;

            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = true;
            }
        }

        private void UpdateLifetime()
        {
            if (IsHost == false || _isWaitingForDieAnimationEnd || _rootBinderStats.IsDead)
            {
                return;
            }

            if (Time.time < _lifeEndTime)
            {
                return;
            }

            _rootBinderStats.PlayerHpValueChangedRpc(0);
            _rootBinderStats.IsDeadValueChangedRpc(true);
        }

        private void ResetForPoolReuse()
        {
            ResetLocalState();

            if (_animator != null)
            {
                _animator.Rebind();
                _animator.Update(0f);
            }
        }

        private void EnterDeadState()
        {
            _hasTriggered = true;
            _isWaitingForDieAnimationEnd = true;

            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = false;
            }

            if (_animator != null)
            {
                _animator.CrossFade(DieAnimHash, 0.05f, 0);
            }
        }

        private void UpdateDead()
        {
            if (_isWaitingForDieAnimationEnd == false)
            {
                return;
            }

            if (_animator == null)
            {
                _isWaitingForDieAnimationEnd = false;

                if (IsHost)
                {
                    _resourcesServices.DestroyObject(gameObject);
                }

                return;
            }

            if (_animator.IsInTransition(0))
            {
                return;
            }

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != DieAnimHash)
            {
                return;
            }

            if (stateInfo.normalizedTime < DieDespawnNormalizedTime)
            {
                return;
            }

            _isWaitingForDieAnimationEnd = false;

            if (IsHost)
            {
                _resourcesServices.DestroyObject(gameObject);
            }
        }

        private void TryTriggerRootDebuff(Collider other)
        {
            if (IsHost == false || _hasTriggered || _isWaitingForDieAnimationEnd || other == null)
            {
                return;
            }

            PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
            if (playerStats == null)
            {
                return;
            }

            if (playerStats.TryGetComponent(out ICCReceiver crowdControlReceiver) == false)
            {
                Debug.Assert(false, $"{playerStats.gameObject.name} hasn't implemented ICCReceiver");
                return;
            }

            _hasTriggered = true;

            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = false;
            }

            crowdControlReceiver.ApplyCC(CCType.Root, gameObject, RootDebuffVfxDuration);

            _vfxManagerServices.InstantiateParticleWithTarget(
                RootDebuffVfxPath,
                playerStats.transform,
                RootDebuffVfxDuration,
                true);

            _resourcesServices.DestroyObject(gameObject);
        }
    }
}
