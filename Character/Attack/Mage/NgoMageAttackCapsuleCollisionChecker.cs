using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using NetWork.BaseNGO;
using Stats;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Character.Attack.Mage
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NgoMageAttackInitialize))]
    public class NgoMageAttackCapsuleCollisionChecker : NetworkBehaviour
    {
        private const string MageAttackHitPath = "Prefabs/Player/VFX/Mage/MageAttackHit";

        [SerializeField] private float _moveSpeed = 8f;

        private LayerMask _collisionLayer;
        private IResourcesServices _resourcesServices;
        private IVFXManagerServices _vfxManagerServices;
        private NgoMageAttackInitialize _initialize;
        private NgoPoolingInitializeBase _poolingInitializeBase;
        private Collider _triggerCollider;
        private bool _hasHit;

        [Inject]
        public void Construct(IResourcesServices resourcesServices, IVFXManagerServices vfxManagerServices)
        {
            _resourcesServices = resourcesServices;
            _vfxManagerServices = vfxManagerServices;
        }

        private void Awake()
        {
            _collisionLayer = LayerMask.GetMask("Monster", "Wall");
            _initialize = GetComponent<NgoMageAttackInitialize>();
            _triggerCollider = GetComponent<Collider>();
            TryGetComponent(out _poolingInitializeBase);

            if (_poolingInitializeBase != null)
            {
                _poolingInitializeBase.PoolObjectReleaseEvent += ResetState;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (_poolingInitializeBase != null)
            {
                _poolingInitializeBase.PoolObjectReleaseEvent -= ResetState;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResetState();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ResetState();
        }

        private void Update()
        {
            if (_hasHit || _initialize == null || _initialize.Caller == null)
            {
                return;
            }

            transform.Translate(Vector3.forward * (_moveSpeed * Time.deltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsServer == false || _hasHit)
            {
                return;
            }

            PlayerStats caller = _initialize != null ? _initialize.Caller : null;
            if (caller == null)
            {
                return;
            }

            if (IsTargetLayer(other.gameObject.layer) == false)
            {
                return;
            }

            _hasHit = true;

            if (TryGetDamageable(other, out IDamageable damageable))
            {
                damageable.OnAttacked(caller, caller.Attack);
            }

            _vfxManagerServices.InstantiateParticleInArea(MageAttackHitPath, transform.position);
            _resourcesServices.DestroyObject(gameObject);
        }

        private bool IsTargetLayer(int layer)
        {
            return (_collisionLayer.value & (1 << layer)) != 0;
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
