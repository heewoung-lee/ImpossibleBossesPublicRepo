using System.Collections;
using Controller;
using NetWork.BaseNGO;
using NetWork.Sync;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Util;

namespace NetWork.BossRedDragon_NGO
{
    public class NGOBossRedDragonNetworkController : NGOBossNetworkController
    {
        private const float MaxJumpCatchUpSpeed = 3f;
        private const float JumpCatchUpDuration = 0.2f;
        private const float MaxBreathCatchUpSpeed = 3f;
        private const float BreathCatchUpDuration = 0.2f;

        private readonly NetworkVariable<bool> _isAirborne = new NetworkVariable<bool>(false);
        private readonly NetworkVariable<bool> _isBreathLooping = new NetworkVariable<bool>(false);

        private NavMeshAgent _agent;
        private RedDragonBreathColliderMarker _breathColliderMarker;
        private RedDragonBreathDamageDealer _breathDamageDealer;
        private Collider _breathCollider;
        private Collider[] _ownedColliders;
        private Coroutine _jumpAnimationCoroutine;
        private Coroutine _breathStartAnimationCoroutine;
        private bool _isJumpCatchUpInitialized;
        private float _remainingJumpCatchUpTime;
        private bool _isBreathCatchUpInitialized;
        private float _remainingBreathCatchUpTime;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            _agent = GetComponent<NavMeshAgent>();
            CacheOwnedColliders();
            CacheBreathCollider();
            ApplyBreathColliderState(false);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _isAirborne.OnValueChanged += OnChangedAirborneState;
            _isBreathLooping.OnValueChanged += OnChangedBreathLoopState;
            ApplyAirborneState(_isAirborne.Value);
            ApplyBreathColliderState(_isBreathLooping.Value);
            InitPosAndRotation();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _isAirborne.OnValueChanged -= OnChangedAirborneState;
            _isBreathLooping.OnValueChanged -= OnChangedBreathLoopState;
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void StartJumpAnimChangedRpc(NetworkAnimationInfo animinfo)
        {
            if (_jumpAnimationCoroutine != null)
            {
                StopCoroutine(_jumpAnimationCoroutine);
            }

            _isJumpCatchUpInitialized = false;
            _remainingJumpCatchUpTime = 0f;
            _bossController.Anim.speed = animinfo.StartAnimationSpeed;
            _jumpAnimationCoroutine = StartCoroutine(UpdateJumpAnimCoroutine(animinfo));
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void StartBreathStartAnimRpc(float animLength, float animationSpeed, double serverTime)
        {
            if (_breathStartAnimationCoroutine != null)
            {
                StopCoroutine(_breathStartAnimationCoroutine);
            }

            _isBreathCatchUpInitialized = false;
            _remainingBreathCatchUpTime = 0f;

            NetworkAnimationInfo animInfo = new NetworkAnimationInfo(
                animLength,
                1f,
                0f,
                0f,
                serverTime,
                animationSpeed);

            _bossController.Anim.speed = animInfo.StartAnimationSpeed;
            _breathStartAnimationCoroutine = StartCoroutine(UpdateBreathStartAnimCoroutine(animInfo));
        }

        public void SetAirborneState(bool isAirborne)
        {
            if (IsHost == false || _isAirborne.Value == isAirborne)
            {
                return;
            }

            _isAirborne.Value = isAirborne;
        }

        public void SetBreathLoopState(bool isBreathLooping)
        {
            if (IsHost == false || _isBreathLooping.Value == isBreathLooping)
            {
                return;
            }

            _isBreathLooping.Value = isBreathLooping;
        }

        public Transform GetBreathAttachTransform()
        {
            CacheBreathCollider();
            return _breathColliderMarker != null ? _breathColliderMarker.transform : null;
        }

        private void OnChangedAirborneState(bool previousValue, bool newValue)
        {
            ApplyAirborneState(newValue);
        }

        private void OnChangedBreathLoopState(bool previousValue, bool newValue)
        {
            ApplyBreathColliderState(newValue);
        }

        private void ApplyAirborneState(bool isAirborne)
        {
            ApplyOwnedColliderState(isAirborne);

            if (_agent == null)
            {
                return;
            }

            if (isAirborne)
            {
                if (_agent.enabled == false)
                {
                    return;
                }

                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _agent.ResetPath();
                _agent.enabled = false;
                return;
            }

            if (_agent.enabled)
            {
                return;
            }

            _agent.enabled = true;
            _agent.Warp(transform.position);
            _agent.velocity = Vector3.zero;
            _agent.ResetPath();
            _agent.isStopped = true;
        }

        private void CacheOwnedColliders()
        {
            if (_ownedColliders != null)
            {
                return;
            }

            _ownedColliders = GetComponentsInChildren<Collider>(true);
        }

        private void ApplyOwnedColliderState(bool isAirborne)
        {
            CacheOwnedColliders();
            if (_ownedColliders == null || _ownedColliders.Length == 0)
            {
                return;
            }

            for (int i = 0; i < _ownedColliders.Length; i++)
            {
                Collider ownedCollider = _ownedColliders[i];
                if (ownedCollider == null)
                {
                    continue;
                }

                ownedCollider.enabled = isAirborne == false;
            }
        }

        private IEnumerator UpdateJumpAnimCoroutine(NetworkAnimationInfo animinfo)
        {
            double elapsedTime = 0f;
            double nowTime = _relayManager.NetworkManagerEx.ServerTime.Time;

            while (elapsedTime <= animinfo.AnimLength)
            {
                double currentNetTime = _relayManager.NetworkManagerEx.ServerTime.Time;
                double deltaTime = currentNetTime - nowTime;
                nowTime = currentNetTime;

                float speedMultiplier = AnimationCatchUpCalculator.ConsumeSpeedMultiplier(
                    _relayManager.NetworkManagerEx,
                    animinfo.ServerTime,
                    ref _isJumpCatchUpInitialized,
                    ref _remainingJumpCatchUpTime,
                    (float)deltaTime,
                    JumpCatchUpDuration,
                    MaxJumpCatchUpSpeed);

                _bossController.Anim.speed = animinfo.StartAnimationSpeed * speedMultiplier;
                elapsedTime += deltaTime * _bossController.Anim.speed;

                yield return null;
            }

            _jumpAnimationCoroutine = null;
            _bossController.Anim.speed = 1f;
        }

        private IEnumerator UpdateBreathStartAnimCoroutine(NetworkAnimationInfo animinfo)
        {
            double elapsedTime = 0f;
            double nowTime = _relayManager.NetworkManagerEx.ServerTime.Time;

            while (elapsedTime <= animinfo.AnimLength)
            {
                double currentNetTime = _relayManager.NetworkManagerEx.ServerTime.Time;
                double deltaTime = currentNetTime - nowTime;
                nowTime = currentNetTime;

                float speedMultiplier = AnimationCatchUpCalculator.ConsumeSpeedMultiplier(
                    _relayManager.NetworkManagerEx,
                    animinfo.ServerTime,
                    ref _isBreathCatchUpInitialized,
                    ref _remainingBreathCatchUpTime,
                    (float)deltaTime,
                    BreathCatchUpDuration,
                    MaxBreathCatchUpSpeed);

                _bossController.Anim.speed = animinfo.StartAnimationSpeed * speedMultiplier;
                elapsedTime += deltaTime * _bossController.Anim.speed;

                yield return null;
            }

            _breathStartAnimationCoroutine = null;
            _bossController.Anim.speed = 1f;
        }

        private void CacheBreathCollider()
        {
            if (_breathCollider != null && _breathColliderMarker != null)
            {
                return;
            }

            RedDragonBreathColliderMarker marker = GetComponentInChildren<RedDragonBreathColliderMarker>(true);
            if (marker == null)
            {
                UtilDebug.LogError($"[{nameof(NGOBossRedDragonNetworkController)}] {nameof(RedDragonBreathColliderMarker)} not found.");
                return;
            }

            if (marker.TryGetComponent(out Collider breathCollider) == false)
            {
                UtilDebug.LogError($"[{nameof(NGOBossRedDragonNetworkController)}] Collider is missing on {marker.name}.");
                return;
            }

            if (marker.TryGetComponent(out RedDragonBreathDamageDealer breathDamageDealer) == false)
            {
                UtilDebug.LogError($"[{nameof(NGOBossRedDragonNetworkController)}] {nameof(RedDragonBreathDamageDealer)} is missing on {marker.name}.");
            }

            _breathColliderMarker = marker;
            _breathDamageDealer = breathDamageDealer;
            _breathCollider = breathCollider;
        }

        private void ApplyBreathColliderState(bool isActive)
        {
            CacheBreathCollider();
            if (_breathDamageDealer != null)
            {
                _breathDamageDealer.SetLoopState(isActive);
                return;
            }

            if (_breathCollider == null)
            {
                return;
            }

            _breathCollider.enabled = isActive;
        }

        private void InitPosAndRotation()
        {
        }
    }
}
