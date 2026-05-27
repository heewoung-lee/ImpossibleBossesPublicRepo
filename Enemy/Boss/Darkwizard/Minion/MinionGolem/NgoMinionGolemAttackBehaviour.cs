using System.Collections;
using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using Stats;
using Stats.BaseStats;
using Stats.MonsterStats;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Enemy.Boss.Darkwizard.Minion.MinionGolem
{
    public class NgoMinionGolemAttackBehaviour : NetworkBehaviour
    {
        private const string MinionGolemAttackHitPath = "Prefabs/Enemy/Minion/MinionGolemAttackHit";

        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _lifeTime = 4f;

        private IResourcesServices _resourcesServices;
        private IVFXManagerServices _vfxManagerServices;
        private LayerMask _collisionLayer;
        private MinionGolemStats _attacker;
        private Coroutine _moveCoroutine;
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
            _collisionLayer = LayerMask.GetMask(
                LayerMask.LayerToName(LayerMask.NameToLayer("Player")),
                LayerMask.LayerToName(LayerMask.NameToLayer("AnotherPlayer")),
                LayerMask.LayerToName(LayerMask.NameToLayer("Wall")));
            _triggerCollider = GetComponent<Collider>();
        }

        public void Initialize(MinionGolemStats attacker, Vector3 fireDirection)
        {
            _attacker = attacker;

            if (fireDirection.sqrMagnitude <= 0.0001f)
            {
                fireDirection = Vector3.forward;
            }
            else
            {
                fireDirection.Normalize();
            }

            transform.rotation = Quaternion.LookRotation(fireDirection, Vector3.up);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResetState();

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(MoveRoutine());
            _resourcesServices.DestroyObject(gameObject, _lifeTime);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ResetState();

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }

            _attacker = null;
        }

        private IEnumerator MoveRoutine()
        {
            while (true)
            {
                if (_hasHit || _attacker == null)
                {
                    yield return null;
                    continue;
                }

                float moveDistance = _moveSpeed * Time.deltaTime;

                transform.Translate(Vector3.forward * moveDistance, Space.Self);
                yield return null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsServer == false || _hasHit || _attacker == null)
            {
                return;
            }

            if (IsTargetLayer(other.gameObject.layer) == false)
            {
                return;
            }

            OnHit(other);
        }

        private bool IsTargetLayer(int layer)
        {
            return (_collisionLayer.value & (1 << layer)) != 0;
        }

        private void OnHit(Collider other)
        {
            _hasHit = true;

            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = false;
            }

            PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.OnAttacked(_attacker, _attacker.Attack);
            }

            _vfxManagerServices.InstantiateParticleInArea(MinionGolemAttackHitPath, transform.position);
            _resourcesServices.DestroyObject(gameObject);
        }

        private void ResetState()
        {
            _hasHit = false;

            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = true;
            }
        }
    }
}
