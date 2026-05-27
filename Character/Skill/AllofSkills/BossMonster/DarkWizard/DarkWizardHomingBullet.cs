using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using NetWork.Sync;
using Stats.BaseStats;
using Stats.BossStats;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Character.Skill.AllofSkills.BossMonster.DarkWizard
{
    public class DarkWizardHomingBullet : NetworkBehaviour
    {
        private const string NGODarkWizardAttackHit = "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardAttackHit";

        [Header("Settings")] public float moveSpeed = 5f;
        [SerializeField] private float turnSpeed = 2f;
        [Header("Distance Sync")]
        [SerializeField] private float catchUpDuration = 0.2f;
        [SerializeField] private float maxCatchUpMultiplier = 3f;
        [SerializeField] private LayerMask hitLayer;

        private BossStats _attacker;
        private Transform _target;
        private bool _isCatchUpInitialized;
        private float _remainingCatchUpDistance;
        private double _serverStartTime;
        private Collider _triggerCollider;
        private bool _hasHit;

        private IResourcesServices _resourcesServices;
        private IVFXManagerServices _vfxManager;

        [Inject]
        public void Construct(IResourcesServices resourcesServices, IVFXManagerServices vfxManager)
        {
            _resourcesServices = resourcesServices;
            _vfxManager = vfxManager;
        }

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _target = null;
            _attacker = null;
            _isCatchUpInitialized = false;
            _remainingCatchUpDistance = 0f;
            _serverStartTime = 0d;
            ResetState();
            _resourcesServices.DestroyObject(gameObject, 7f);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ResetState();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void FireRpc(NetworkObjectReference attacker, NetworkObjectReference target, double spawnServerTime)
        {
            if (attacker.TryGet(out NetworkObject attackerObj))
            {
                _attacker = attackerObj.transform.GetComponent<BossStats>();
            }

            if (target.TryGet(out NetworkObject targetObj))
            {
                _target = targetObj.transform;
            }

            if (NetworkManager == null)
            {
                _remainingCatchUpDistance = 0f;
                return;
            }

            _serverStartTime = spawnServerTime;
            _isCatchUpInitialized = false;
            _remainingCatchUpDistance = 0f;
        }

        private void FixedUpdate()
        {
            if (_target == null || _hasHit)
            {
                return;
            }

            PredictMovement();
        }

        private void PredictMovement()
        {
            float deltaTime = Time.fixedDeltaTime;
            Vector3 directionToTarget = (_target.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * deltaTime);

            float baseDistance = moveSpeed * deltaTime;
            float extraDistance = ChaseCatchUpCalculator.ConsumeExtraDistance(
                NetworkManager,
                _serverStartTime,
                ref _isCatchUpInitialized,
                ref _remainingCatchUpDistance,
                deltaTime,
                moveSpeed,
                catchUpDuration,
                maxCatchUpMultiplier);

            float finalDistance = baseDistance + extraDistance;
            transform.position += transform.forward * finalDistance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsServer == false || _hasHit || _target == null)
            {
                return;
            }

            if (IsTargetLayer(other.gameObject.layer) == false)
            {
                return;
            }

            HandleCollision(other);
        }

        private bool IsTargetLayer(int layer)
        {
            return (hitLayer.value & (1 << layer)) != 0;
        }

        private void HandleCollision(Collider other)
        {
            _hasHit = true;

            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = false;
            }

            _resourcesServices.DestroyObject(gameObject);
            _vfxManager.InstantiateParticleInArea(NGODarkWizardAttackHit, transform.position);

            if (_attacker != null && TryGetDamageable(other, out IDamageable damageable))
            {
                damageable.OnAttacked(_attacker);
            }
        }

        private void ResetState()
        {
            _hasHit = false;

            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = true;
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
    }
}
