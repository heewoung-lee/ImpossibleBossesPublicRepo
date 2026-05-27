using GameManagers.ResourcesExManagement;
using GameManagers.RelayManagement;
using GameManagers.VFXManagement;
using NetWork.BaseNGO;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace Character.Skill.AllofSkills.BossMonster.StoneGolem
{
    public class NgoBossSkill1AttackBehaviour : NetworkBehaviour
    {
        private const string HitVfxPath = "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoBossSkill1AttackHit";
        private const float MinFlightDuration = 0.1f;

        [SerializeField] private float _maxHeight = 3f;
        [SerializeField] private float _collisionRadius = 0.35f;

        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        private IVFXManagerServices _vfxManagerServices;
        private NgoPoolingInitializeBase _poolingInitializeBase;
        private ParticleSystem[] _particleSystems;
        private IAttackRange _attacker;
        private LayerMask _collisionMask;
        private bool _isConfigured;
        private bool _hasHit;
        private bool _hasImpactOnArrival;
        private float _flightDuration;
        private int _damage;
        private double _startServerTime;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private Vector3 _previousPosition;

        [Inject]
        public void Construct(
            IResourcesServices resourcesServices,
            RelayManager relayManager,
            IVFXManagerServices vfxManagerServices)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
            _vfxManagerServices = vfxManagerServices;
        }

        private void Awake()
        {
            _poolingInitializeBase = GetComponent<NgoPoolingInitializeBase>();
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            _poolingInitializeBase.PoolObjectReleaseEvent += ResetProjectileState;
        }

        public override void OnDestroy()
        {
            if (_poolingInitializeBase != null)
            {
                _poolingInitializeBase.PoolObjectReleaseEvent -= ResetProjectileState;
            }

            base.OnDestroy();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResetProjectileState();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ResetProjectileState();
        }

        private void Update()
        {
            if (_isConfigured == false || _relayManager == null)
            {
                return;
            }

            double elapsedTime = _relayManager.NetworkManagerEx.ServerTime.Time - _startServerTime;
            float normalizedTime = Mathf.Clamp01((float)(elapsedTime / _flightDuration));
            Vector3 currentPosition = EvaluatePosition(normalizedTime);

            if (IsServer && _hasHit == false && _hasImpactOnArrival)
            {
                TryHitAlongPath(_previousPosition, currentPosition);
            }

            if (_hasHit)
            {
                return;
            }

            transform.position = currentPosition;
            _previousPosition = currentPosition;

            if (normalizedTime >= 1f && IsServer)
            {
                if (_hasImpactOnArrival)
                {
                    HandleHit(null, _targetPosition);
                }
                else
                {
                    ReleaseProjectile();
                }
            }
        }

        public void ConfigureLaunchOnHost(
            float duration,
            Vector3 targetPosition,
            ulong attackerNetworkObjectId,
            int damage,
            bool hasImpactOnArrival)
        {
            if (IsHost == false)
            {
                return;
            }

            _damage = damage;
            _hasImpactOnArrival = hasImpactOnArrival;
            _attacker = null;
            _collisionMask = 0;

            if (_hasImpactOnArrival)
            {
                if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(attackerNetworkObjectId,
                        out NetworkObject attackerObject) == false)
                {
                    throw new MissingComponentException(
                        $"[{nameof(NgoBossSkill1AttackBehaviour)}] attacker network object is missing. id:{attackerNetworkObjectId}");
                }

                if (attackerObject.TryGetComponent(out IAttackRange attacker) == false)
                {
                    throw new MissingComponentException(
                        $"[{nameof(NgoBossSkill1AttackBehaviour)}] attacker is missing {nameof(IAttackRange)}.");
                }

                _attacker = attacker;
                _collisionMask = attacker.TarGetLayer | LayerMask.GetMask("Ground");
            }

            StartLaunchRpc(
                transform.position,
                targetPosition,
                Mathf.Max(duration, MinFlightDuration),
                _relayManager.NetworkManagerEx.ServerTime.Time,
                _hasImpactOnArrival);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void StartLaunchRpc(
            Vector3 startPosition,
            Vector3 targetPosition,
            float flightDuration,
            double startServerTime,
            bool hasImpactOnArrival)
        {
            _startPosition = startPosition;
            _targetPosition = targetPosition;
            _flightDuration = Mathf.Max(flightDuration, MinFlightDuration);
            _startServerTime = startServerTime;
            _previousPosition = startPosition;
            _isConfigured = true;
            _hasHit = false;
            _hasImpactOnArrival = hasImpactOnArrival;

            transform.position = startPosition;
            PlayParticles();
        }

        private void TryHitAlongPath(Vector3 fromPosition, Vector3 toPosition)
        {
            Vector3 direction = toPosition - fromPosition;
            float distance = direction.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return;
            }

            if (Physics.SphereCast(
                    fromPosition,
                    _collisionRadius,
                    direction.normalized,
                    out RaycastHit hit,
                    distance,
                    _collisionMask,
                    QueryTriggerInteraction.Ignore) == false)
            {
                return;
            }

            HandleHit(hit.collider, hit.point);
        }

        private void HandleHit(Collider hitCollider, Vector3 hitPoint)
        {
            if (_hasHit)
            {
                return;
            }

            _hasHit = true;
            _isConfigured = false;

            if (hitCollider != null && TryGetDamageable(hitCollider, out IDamageable damageable))
            {
                damageable.OnAttacked(_attacker, _damage);
            }

            _vfxManagerServices.InstantiateParticleInArea(HitVfxPath, hitPoint);
            ReleaseProjectile();
        }

        private void ReleaseProjectile()
        {
            _hasHit = true;
            _isConfigured = false;
            _resourcesServices.DestroyObject(gameObject);
        }

        private Vector3 EvaluatePosition(float normalizedTime)
        {
            if (_hasImpactOnArrival)
            {
                return Vector3.Lerp(_startPosition, _targetPosition, normalizedTime);
            }

            Vector3 position = Vector3.Lerp(_startPosition, _targetPosition, normalizedTime);
            position.y = Mathf.Lerp(_startPosition.y, _targetPosition.y, normalizedTime) +
                         _maxHeight * Mathf.Sin(Mathf.PI * normalizedTime);
            return position;
        }

        private static bool TryGetDamageable(Collider hitCollider, out IDamageable damageable)
        {
            MonoBehaviour[] parentBehaviours = hitCollider.GetComponentsInParent<MonoBehaviour>();
            for (int i = 0; i < parentBehaviours.Length; i++)
            {
                if (parentBehaviours[i] is IDamageable targetDamageable)
                {
                    damageable = targetDamageable;
                    return true;
                }
            }

            damageable = null;
            return false;
        }

        private void PlayParticles()
        {
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                _particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _particleSystems[i].Play(true);
            }
        }

        private void ResetProjectileState()
        {
            _attacker = null;
            _collisionMask = 0;
            _isConfigured = false;
            _hasHit = false;
            _hasImpactOnArrival = false;
            _flightDuration = 0f;
            _damage = 0;
            _startServerTime = 0d;
            _startPosition = Vector3.zero;
            _targetPosition = Vector3.zero;
            _previousPosition = Vector3.zero;

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                _particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
