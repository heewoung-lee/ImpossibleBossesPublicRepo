using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using Stats.BaseStats;
using Stats.BossStats;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Enemy.Boss.Darkwizard
{
    public class NgoDarkWizardSectorAttackCollisionBehaviour : NetworkBehaviour
    {
        private const string SectorAttackHitPath = "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardSectorAttackHit";

        private IResourcesServices _resourcesServices;
        private IVFXManagerServices _vfxManagerServices;

        private LayerMask _collisionLayer;
        private BossDarkWizardStats _attacker;
        private Collider _triggerCollider;

        [Inject]
        public void Construct(IResourcesServices resourcesServices, IVFXManagerServices vfxManagerServices)
        {
            _resourcesServices = resourcesServices;
            _vfxManagerServices = vfxManagerServices;
        }

        private void Awake()
        {
            _collisionLayer = LayerMask.GetMask("Player", "AnotherPlayer", "Wall");
            _triggerCollider = GetComponent<Collider>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _attacker = null;
            SetTriggerEnabled(false);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _attacker = null;
            SetTriggerEnabled(false);
        }

        public void Initialize(BossDarkWizardStats attacker)
        {
            _attacker = attacker;
            SetTriggerEnabled(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsServer == false)
            {
                return;
            }

            if (IsTargetLayer(other.gameObject.layer) == false)
            {
                return;
            }

            SetTriggerEnabled(false);

            if (_attacker != null && TryGetDamageable(other, out IDamageable damageable))
            {
                damageable.OnAttacked(_attacker, _attacker.Attack);
            }

            _vfxManagerServices.InstantiateParticleInArea(SectorAttackHitPath, transform.position);
            _resourcesServices.DestroyObject(gameObject);
        }

        private bool IsTargetLayer(int layer)
        {
            return (_collisionLayer.value & (1 << layer)) != 0;
        }

        private void SetTriggerEnabled(bool isEnabled)
        {
            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = isEnabled;
            }
        }

        private bool TryGetDamageable(Collider other, out IDamageable damageable)
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
