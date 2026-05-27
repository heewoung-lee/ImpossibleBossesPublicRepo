using GameManagers.ResourcesExManagement;
using GameManagers.RelayManagement;
using GameManagers.VFXManagement;
using NetWork.BaseNGO;
using Stats.BaseStats;
using Stats.BossStats;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class NgoRedDragonProjectileLifetimeBehaviour : NetworkBehaviour
    {
        private const string ProjectileHitVfxPath =
            "Prefabs/Enemy/Boss/AttackPattern/RedDragon/RedDragonProjectileHitVFX";

        [SerializeField] private float _lifeTime = 0.5f;
        [SerializeField] private float _launchSpeed = 12f;

        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        private IVFXManagerServices _vfxManager;
        private NgoPoolingInitializeBase _poolingInitializeBase;
        private BossStats _attacker;
        private LayerMask _collisionLayer;
        private Collider _hitCollider;
        private int _damage;

        private bool _isScatterConfigured;
        private bool _hasLaunched;
        private bool _hasHit;
        private float _rotationDuration;
        private double _startServerTime;
        private Vector3 _spawnPosition;
        private Quaternion _startRotation = Quaternion.identity;
        private Quaternion _targetRotation = Quaternion.identity;

        [Inject]
        public void Construct(
            IResourcesServices resourcesServices,
            RelayManager relayManager,
            IVFXManagerServices vfxManager)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
            _vfxManager = vfxManager;
        }

        private void Awake()
        {
            TryGetComponent(out _poolingInitializeBase);
            _hitCollider = GetComponent<Collider>();
            if (_poolingInitializeBase != null)
            {
                _poolingInitializeBase.PoolObjectReleaseEvent += ResetScatterState;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResetScatterState();
            _resourcesServices.DestroyObject(gameObject, _lifeTime);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ResetScatterState();
        }

        private void Update()
        {
            if (_isScatterConfigured == false || _relayManager == null)
            {
                return;
            }

            double currentServerTime = _relayManager.NetworkManagerEx.ServerTime.Time;
            double elapsedTime = currentServerTime - _startServerTime;
            ApplyScatterState(elapsedTime);
            UpdateLaunchState(elapsedTime);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (_poolingInitializeBase != null)
            {
                _poolingInitializeBase.PoolObjectReleaseEvent -= ResetScatterState;
            }
        }

        public void StartScatter(
            Vector3 spawnPosition,
            Quaternion spawnRotation,
            float rotationDuration,
            float rotationAngle,
            BossStats attacker,
            int damage)
        {
            if (IsHost == false || _relayManager == null)
            {
                return;
            }

            _attacker = attacker;
            _damage = damage;
            _collisionLayer = BuildCollisionLayer();

            StartScatterRpc(
                spawnPosition,
                spawnRotation.eulerAngles.y,
                Mathf.Max(rotationDuration, 0f),
                rotationAngle,
                _relayManager.NetworkManagerEx.ServerTime.Time);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void StartScatterRpc(
            Vector3 spawnPosition,
            float spawnYaw,
            float rotationDuration,
            float rotationAngle,
            double startServerTime)
        {
            _spawnPosition = spawnPosition;
            _startRotation = Quaternion.Euler(0f, spawnYaw, 0f);
            _targetRotation = Quaternion.Euler(0f, spawnYaw + rotationAngle, 0f);
            _rotationDuration = Mathf.Max(rotationDuration, 0f);
            _startServerTime = startServerTime;
            _isScatterConfigured = true;

            transform.position = _spawnPosition;
            transform.rotation = _startRotation;

            double elapsedTime = 0d;
            if (_relayManager != null)
            {
                elapsedTime = _relayManager.NetworkManagerEx.ServerTime.Time - _startServerTime;
            }

            ApplyScatterState(elapsedTime);
            UpdateLaunchState(elapsedTime);
            ScheduleLifetime(elapsedTime);
        }

        private void ApplyScatterState(double elapsedTime)
        {
            float elapsed = Mathf.Max(0f, (float)elapsedTime);
            float normalizedRotationTime = _rotationDuration <= Mathf.Epsilon
                ? 1f
                : Mathf.Clamp01(elapsed / _rotationDuration);

            transform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, normalizedRotationTime);

            if (elapsed <= _rotationDuration)
            {
                transform.position = _spawnPosition;
                return;
            }

            float launchElapsedTime = elapsed - _rotationDuration;
            Vector3 launchDirection = _targetRotation * Vector3.forward;
            transform.position = _spawnPosition + launchDirection * (_launchSpeed * launchElapsedTime);
        }

        private void UpdateLaunchState(double elapsedTime)
        {
            bool shouldLaunch = elapsedTime >= _rotationDuration;
            if (_hasLaunched == shouldLaunch)
            {
                return;
            }

            _hasLaunched = shouldLaunch;
            SetHitColliderEnabled(_hasLaunched && IsServer && _hasHit == false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsServer == false || _hasLaunched == false || _hasHit)
            {
                return;
            }

            if (IsTargetLayer(other.gameObject.layer) == false)
            {
                return;
            }

            _hasHit = true;
            SetHitColliderEnabled(false);

            if (_attacker != null && TryGetDamageable(other, out IDamageable damageable))
            {
                damageable.OnAttacked(_attacker, _damage);
            }

            _vfxManager.InstantiateParticleInArea(ProjectileHitVfxPath, transform.position);
            _resourcesServices.DestroyObject(gameObject);
        }

        private void ScheduleLifetime(double elapsedTime)
        {
            float totalLifetime = _rotationDuration + Mathf.Max(_lifeTime, 0f);
            float remainingLifetime = Mathf.Max(0f, totalLifetime - Mathf.Max(0f, (float)elapsedTime));
            _resourcesServices.DestroyObject(gameObject, remainingLifetime);
        }

        private LayerMask BuildCollisionLayer()
        {
            int fallbackMask = LayerMask.GetMask(
                Utill.GetLayerID(Define.ControllerLayer.Player),
                Utill.GetLayerID(Define.ControllerLayer.AnotherPlayer),
                "Wall");

            if (_attacker == null)
            {
                return fallbackMask;
            }

            int targetMask = _attacker.TarGetLayer.value;
            if (targetMask == 0)
            {
                return fallbackMask;
            }

            return targetMask | LayerMask.GetMask("Wall");
        }

        private bool IsTargetLayer(int layer)
        {
            return (_collisionLayer.value & (1 << layer)) != 0;
        }

        private void SetHitColliderEnabled(bool isEnabled)
        {
            if (_hitCollider != null)
            {
                _hitCollider.enabled = isEnabled;
            }
        }

        private static bool TryGetDamageable(Collider other, out IDamageable damageable)
        {
            MonoBehaviour[] parentBehaviours = other.GetComponentsInParent<MonoBehaviour>();
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

        private void ResetScatterState()
        {
            _attacker = null;
            _damage = 0;
            _collisionLayer = 0;
            _isScatterConfigured = false;
            _hasLaunched = false;
            _hasHit = false;
            _rotationDuration = 0f;
            _startServerTime = 0d;
            _spawnPosition = Vector3.zero;
            _startRotation = Quaternion.identity;
            _targetRotation = Quaternion.identity;
            SetHitColliderEnabled(false);
        }
    }
}
