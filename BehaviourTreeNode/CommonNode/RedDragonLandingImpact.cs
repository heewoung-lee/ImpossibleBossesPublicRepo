using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using NetWork;
using NetWork.NGO;
using Stats.BaseStats;
using UnityEngine;
using Util;
using VFX;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonLandingImpact : Action
    {
        private const string LandingVfxPath = "Prefabs/Enemy/Boss/AttackPattern/RedDragon/DragonLandingVFX";
        private const float VerticalCandidatePadding = 3f;

        [SerializeField] private SharedProjector _landingIndicator;
        [SerializeField] private float _vfxDuration = 1f;
        [SerializeField] private int _damage = -1;
        [SerializeField] private float _vfxYOffset = 0f;
        [SerializeField] private float _pushDistance = 3f;
        [SerializeField] private float _pushDuration = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _cameraShakeIntensity = 0.8f;
        [SerializeField, Min(0f)] private float _cameraShakeDuration = 0.25f;

        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        private IAttackRange _attackRange;
        private NgoIndicatorController _indicatorController;
        private GameObject _landingVfxPrefab;
        private bool _hasTriggeredImpact;

        private IResourcesServices ResourcesServices
        {
            get
            {
                if (_resourcesServices == null)
                {
                    _resourcesServices = GetComponent<BossDependencyHub>().ResourcesServices;
                }

                return _resourcesServices;
            }
        }

        private RelayManager RelayManager
        {
            get
            {
                if (_relayManager == null)
                {
                    _relayManager = GetComponent<BossDependencyHub>().RelayManager;
                }

                return _relayManager;
            }
        }

        public override void OnAwake()
        {
            base.OnAwake();
            _attackRange = Owner.GetComponent<IAttackRange>();
            _landingVfxPrefab = ResourcesServices.Load<GameObject>(LandingVfxPath);
        }

        public override void OnStart()
        {
            base.OnStart();
            _indicatorController = _landingIndicator.Value as NgoIndicatorController;
            _hasTriggeredImpact = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (_indicatorController == null)
            {
                return TaskStatus.Failure;
            }

            if (_hasTriggeredImpact)
            {
                return TaskStatus.Success;
            }

            if (_indicatorController.NormalizedProgress < 1f)
            {
                return TaskStatus.Running;
            }

            TriggerImpact();
            _hasTriggeredImpact = true;
            return TaskStatus.Success;
        }

        private void TriggerImpact()
        {
            if (RelayManager.NetworkManagerEx.IsHost == false)
            {
                return;
            }

            SpawnLandingVfx();
            ApplyLandingDamage();
            RelayManager.NgoRPCCaller.RequestCameraShakeRpc(_cameraShakeIntensity, _cameraShakeDuration);
        }

        private void SpawnLandingVfx()
        {
            Vector3 spawnScale = _landingVfxPrefab != null ? _landingVfxPrefab.transform.localScale : Vector3.one;
            Vector3 spawnPosition = ResolveImpactGroundPosition();
            NetworkParams networkParams = new NetworkParams(argFloat: _vfxDuration);
            RelayManager.NgoRPCCaller.SpawnVFXPrefabServerRpc(
                LandingVfxPath,
                _vfxDuration,
                spawnPosition,
                Quaternion.identity,
                spawnScale,
                networkParams);
        }

        private Vector3 ResolveImpactGroundPosition()
        {
            Vector3 groundPosition = _indicatorController.Position;
            groundPosition.y = _vfxYOffset;
            return groundPosition;
        }

        private void ApplyLandingDamage()
        {
            if (_attackRange == null)
            {
                return;
            }

            Collider[] targets = Physics.OverlapSphere(
                _indicatorController.Position,
                _indicatorController.Radius + VerticalCandidatePadding,
                _attackRange.TarGetLayer);

            HashSet<IDamageable> attackedTargets = new HashSet<IDamageable>();
            for (int i = 0; i < targets.Length; i++)
            {
                Collider target = targets[i];
                IDamageable damageable = targets[i].GetComponentInParent<IDamageable>();
                if (damageable == null || attackedTargets.Contains(damageable))
                {
                    continue;
                }

                float targetPadding = Mathf.Max(target.bounds.extents.x, target.bounds.extents.z);
                if (IsInsideIndicatorCircle(target.bounds.center, _indicatorController.Position, _indicatorController.Radius + targetPadding) == false)
                {
                    continue;
                }

                if (_damage > 0)
                {
                    damageable.OnAttacked(_attackRange, _damage);
                }
                else
                {
                    damageable.OnAttacked(_attackRange);
                }

                ApplyKnockback(target);
                attackedTargets.Add(damageable);
            }
        }

        private static bool IsInsideIndicatorCircle(Vector3 targetPosition, Vector3 indicatorPosition, float radius)
        {
            Vector2 targetPoint = new Vector2(targetPosition.x, targetPosition.z);
            Vector2 indicatorPoint = new Vector2(indicatorPosition.x, indicatorPosition.z);
            return (targetPoint - indicatorPoint).sqrMagnitude <= radius * radius;
        }

        private void ApplyKnockback(Collider target)
        {
            if (_pushDistance <= 0f || _pushDuration <= 0f)
            {
                return;
            }

            if (target.transform.TryGetComponentInParents(out PlayerInitializeNgo playerInitializeNgo) == false)
            {
                return;
            }

            Vector3 pushDir = target.bounds.center - _indicatorController.Position;
            pushDir.y = 0f;
            if (pushDir.sqrMagnitude <= 0.0001f)
            {
                pushDir = Owner.transform.forward;
            }
            else
            {
                pushDir.Normalize();
            }

            playerInitializeNgo.PushBackFromNetworkRpc(pushDir, _pushDistance, _pushDuration);
        }
    }
}
