using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using Controller.BossState.BossRedDragon;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using Module.EnemyModule.Boss.RedDragon;
using NetWork;
using Stats.BaseStats;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Util;
using VFX;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonAttack : Action
    {
        private const string IndicatorPath = "Prefabs/Enemy/Boss/Indicator/NGODragonArrowIndicator";
        private const string IndicatorLineVfxPath = "Prefabs/Enemy/Boss/AttackPattern/RedDragon/RedDragonAttackVFX";
        private const string GroundAttackClipName = "RedDragonGroundAttack";
        private const float AttackAnimStopThreshold = 0.06f;
        private const float AddIndicatorDurationTime = 0f;
        private const float IndicatorLineVfxSpacing = 2f;
        private const float IndicatorLineVfxDuration = 1f;
        private const float MinIndicatorDamageSampleSpacing = 0.25f;
        private const float IndicatorDamageWidthMultiplier = 0.7f;

        [SerializeField] private SharedProjector _attackIndicator;

        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        private BossRedDragonController _controller;
        private NGOBossNetworkController _networkController;
        private RedDragonSoundAnimationEvent _soundAnimationEvent;
        private IAttackRange _attackRange;
        private NgoArrowIndicatorController _arrowIndicatorController;
        private GameObject _redDragonAttackVfxPrefab;
        private float _animLength;

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
            _controller = Owner.GetComponent<BossRedDragonController>();
            _networkController = Owner.GetComponent<NGOBossNetworkController>();
            _soundAnimationEvent = Owner.GetComponent<RedDragonSoundAnimationEvent>();
            _attackRange = Owner.GetComponent<IAttackRange>();
            _redDragonAttackVfxPrefab = ResourcesServices.Load<GameObject>(IndicatorLineVfxPath);
            _animLength = Utill.GetAnimationLength(GroundAttackClipName, _controller.Anim);
        }

        public override void OnStart()
        {
            base.OnStart();
            _controller.UpdateAttack();
            SpawnAttackIndicator();
            StartAnimationSpeedChanged();
        }

        public override TaskStatus OnUpdate()
        {
            return _networkController.FinishAttack ? TaskStatus.Success : TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _controller.UpdateIdle();
        }

        private void SpawnAttackIndicator()
        {
            GameObject indicatorObject = ResourcesServices.InstantiateByKey(IndicatorPath);
            indicatorObject = RelayManager.SpawnNetworkObj(indicatorObject);
            _arrowIndicatorController = indicatorObject.GetComponent<NgoArrowIndicatorController>();

            if (_arrowIndicatorController == null)
            {
                UtilDebug.LogError($"[{nameof(RedDragonAttack)}] {nameof(NgoArrowIndicatorController)} is missing.");
                return;
            }

            _attackIndicator.Value = _arrowIndicatorController;
            if (Owner.TryGetComponent(out NetworkObject ownerNetworkObject))
            {
                _arrowIndicatorController.SetSpawnerBossNetworkObjectId(ownerNetworkObject.NetworkObjectId);
            }

            float totalIndicatorDurationTime = AddIndicatorDurationTime + _animLength;
            _arrowIndicatorController.PlayWithCurrentShape(
                _controller.transform,
                totalIndicatorDurationTime,
                SpawnIndicatorLineVfx);
        }

        private void SpawnIndicatorLineVfx()
        {
            if (_arrowIndicatorController == null)
            {
                return;
            }

            _soundAnimationEvent.PlayAttackSfxFromNode();
            List<Vector3> positions = _arrowIndicatorController.GenerateLineSpawnPositions(IndicatorLineVfxSpacing);
            if (positions.Count == 0)
            {
                return;
            }

            float spawnY = GetRedDragonVfxSpawnY();
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 position = positions[i];
                position.y = spawnY;
                positions[i] = position;
            }

            SpawnGroundAttackVfx(positions);
            ApplyDamageToTargetsInsideIndicator();
        }

        private float GetRedDragonVfxSpawnY()
        {
            if (_controller.TryGetComponent(out Collider bossCollider))
            {
                return bossCollider.bounds.center.y;
            }

            return _controller.transform.position.y;
        }

        private void SpawnGroundAttackVfx(List<Vector3> positions)
        {
            if (positions == null || positions.Count == 0)
            {
                return;
            }

            Vector3 vfxScale = GetGroundAttackVfxScale();
            NetworkParams networkParams = new NetworkParams(argFloat: IndicatorLineVfxDuration) { };
            for (int i = 0; i < positions.Count; i++)
            {
                RelayManager.NgoRPCCaller.SpawnVFXPrefabServerRpc(
                    IndicatorLineVfxPath,
                    IndicatorLineVfxDuration,
                    positions[i],
                    Quaternion.identity,
                    vfxScale,
                    networkParams);
            }
        }

        private Vector3 GetGroundAttackVfxScale()
        {
            if (_redDragonAttackVfxPrefab != null)
            {
                return _redDragonAttackVfxPrefab.transform.localScale;
            }

            return Vector3.one;
        }

        private void ApplyDamageToTargetsInsideIndicator()
        {
            if (RelayManager.NetworkManagerEx.IsHost == false || _arrowIndicatorController == null)
            {
                return;
            }

            float hitRadius = _arrowIndicatorController.GetIndicatorLineHalfWidth()* IndicatorDamageWidthMultiplier;
            float sampleSpacing = Mathf.Max(hitRadius * 0.5f, MinIndicatorDamageSampleSpacing);
            List<Vector3> samplePositions = _arrowIndicatorController.GenerateLineSpawnPositions(sampleSpacing);
            if (samplePositions.Count == 0)
            {
                return;
            }

            Collider[] targets = Physics.OverlapSphere(
                _arrowIndicatorController.Position,
                _arrowIndicatorController.GetIndicatorTotalLength() + hitRadius,
                _attackRange.TarGetLayer);

            HashSet<IDamageable> attackedTargets = new HashSet<IDamageable>();
            for (int i = 0; i < targets.Length; i++)
            {
                Collider target = targets[i];
                IDamageable damageable = target.GetComponentInParent<IDamageable>();
                if (damageable == null || attackedTargets.Contains(damageable))
                {
                    continue;
                }

                Vector3 targetPosition = target.bounds.center;
                float padding = Mathf.Max(target.bounds.extents.x, target.bounds.extents.z);
                if (IsTargetInsideIndicatorSamples(samplePositions, targetPosition, hitRadius + padding) == false)
                {
                    continue;
                }

                damageable.OnAttacked(_attackRange);
                attackedTargets.Add(damageable);
            }
        }

        private static bool IsTargetInsideIndicatorSamples(List<Vector3> samplePositions, Vector3 targetPosition, float hitRadius)
        {
            Vector2 targetPoint = new Vector2(targetPosition.x, targetPosition.z);
            float hitRadiusSquared = hitRadius * hitRadius;

            for (int i = 0; i < samplePositions.Count; i++)
            {
                Vector3 samplePosition = samplePositions[i];
                Vector2 samplePoint = new Vector2(samplePosition.x, samplePosition.z);
                if ((samplePoint - targetPoint).sqrMagnitude <= hitRadiusSquared)
                {
                    return true;
                }
            }

            return false;
        }

        private void StartAnimationSpeedChanged()
        {
            if (_controller.TryGetAttackTypePreTime(_controller.BaseAttackState, out float decelerationRatio) == false)
            {
                return;
            }

            NetworkAnimationInfo animInfo = new NetworkAnimationInfo(
                _animLength,
                decelerationRatio,
                AttackAnimStopThreshold,
                AddIndicatorDurationTime,
                RelayManager.NetworkManagerEx.ServerTime.Time);

            _networkController.StartAnimChangedRpc(animInfo);
        }
    }
}
