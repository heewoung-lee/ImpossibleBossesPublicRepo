using System.Collections;
using System.Collections.Generic;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;

namespace NetWork.BossRedDragon_NGO
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class RedDragonBreathDamageDealer : MonoBehaviour
    {
        private readonly Dictionary<int, float> _lastHitTimeByTarget = new Dictionary<int, float>();
        private readonly Dictionary<int, int> _lastHitFrameByTarget = new Dictionary<int, int>();

        private Collider _triggerCollider;
        private NetworkObject _ownerNetworkObject;
        private IAttackRange _attacker;
        private Coroutine _enableColliderCoroutine;
        private bool _isLoopActive;
        private int _damage;
        private float _colliderEnableDelay;
        private bool _checkDuplicateTargetPerTick;
        private float _damageInterval;

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider>();
            _ownerNetworkObject = GetComponentInParent<NetworkObject>();
            SetColliderEnabled(false);
        }

        public void Configure(
            IAttackRange attacker,
            int damage,
            float colliderEnableDelay,
            bool checkDuplicateTargetPerTick,
            float damageInterval)
        {
            _attacker = attacker;
            _damage = damage;
            _colliderEnableDelay = colliderEnableDelay;
            _checkDuplicateTargetPerTick = checkDuplicateTargetPerTick;
            _damageInterval = damageInterval;
        }

        public void SetLoopState(bool isLoopActive)
        {
            _isLoopActive = isLoopActive;

            if (_enableColliderCoroutine != null)
            {
                StopCoroutine(_enableColliderCoroutine);
                _enableColliderCoroutine = null;
            }

            if (_isLoopActive == false)
            {
                ResetHitCache();
                SetColliderEnabled(false);
                return;
            }

            if (_colliderEnableDelay <= 0f)
            {
                SetColliderEnabled(true);
                return;
            }

            SetColliderEnabled(false);
            _enableColliderCoroutine = StartCoroutine(EnableColliderAfterDelay());
        }

        private IEnumerator EnableColliderAfterDelay()
        {
            yield return new WaitForSeconds(_colliderEnableDelay);
            _enableColliderCoroutine = null;

            if (_isLoopActive == false)
            {
                yield break;
            }

            SetColliderEnabled(true);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_isLoopActive == false || HasServerAuthority() == false || _attacker == null)
            {
                return;
            }

            if (IsTargetLayer(other.gameObject.layer) == false)
            {
                return;
            }

            if (TryGetDamageable(other, out IDamageable damageable, out Component damageableComponent) == false)
            {
                return;
            }

            int targetKey = damageableComponent.GetInstanceID();
            if (CanApplyDamage(targetKey) == false)
            {
                return;
            }

            damageable.OnAttacked(_attacker, _damage);
            _lastHitTimeByTarget[targetKey] = Time.time;
            _lastHitFrameByTarget[targetKey] = Time.frameCount;
        }

        private bool CanApplyDamage(int targetKey)
        {
            if (_checkDuplicateTargetPerTick &&
                _lastHitFrameByTarget.TryGetValue(targetKey, out int lastHitFrame) &&
                lastHitFrame == Time.frameCount)
            {
                return false;
            }

            if (_damageInterval > 0f &&
                _lastHitTimeByTarget.TryGetValue(targetKey, out float lastHitTime) &&
                Time.time - lastHitTime < _damageInterval)
            {
                return false;
            }

            return true;
        }

        private bool HasServerAuthority()
        {
            return _ownerNetworkObject != null &&
                _ownerNetworkObject.NetworkManager != null &&
                _ownerNetworkObject.NetworkManager.IsHost;
        }

        private bool IsTargetLayer(int layer)
        {
            return (_attacker.TarGetLayer.value & (1 << layer)) != 0;
        }

        private static bool TryGetDamageable(Collider other, out IDamageable damageable, out Component damageableComponent)
        {
            MonoBehaviour[] parentBehaviours = other.GetComponentsInParent<MonoBehaviour>();
            for (int i = 0; i < parentBehaviours.Length; i++)
            {
                if (parentBehaviours[i] is IDamageable targetDamageable)
                {
                    damageable = targetDamageable;
                    damageableComponent = parentBehaviours[i];
                    return true;
                }
            }

            damageable = null;
            damageableComponent = null;
            return false;
        }

        private void ResetHitCache()
        {
            _lastHitTimeByTarget.Clear();
            _lastHitFrameByTarget.Clear();
        }

        private void SetColliderEnabled(bool isEnabled)
        {
            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = isEnabled;
            }
        }
    }
}
