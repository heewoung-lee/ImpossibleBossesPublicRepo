using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using Controller.BossState.BossRedDragon;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Util;
using VFX;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonAirBombard : Action
    {
        private const string RainIndicatorPath = "Prefabs/Enemy/Boss/Indicator/NGODragonRainIndicator";
        private const float IndicatorArc = 360f;
        private const float VerticalCandidatePadding = 3f;
        private const float PlayerPredictionDistance = 3f;

        [SerializeField] private float _airborneDuration = 10f;
        [SerializeField] private float _spawnInterval = 0.5f;
        [SerializeField] private float _indicatorRadius = 5f;
        [SerializeField] private float _indicatorDuration = 0.5f;
        [SerializeField] private int _indicatorDamage = -1;

        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        private BossRedDragonController _controller;
        private IAttackRange _attackRange;
        private float _elapsedTime;
        private float _spawnTimer;
        private readonly List<GameObject> _targetPlayers = new List<GameObject>();
        private readonly Dictionary<GameObject, Vector3> _previousTargetPositions = new Dictionary<GameObject, Vector3>();

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
            _attackRange = Owner.GetComponent<IAttackRange>();
        }

        public override void OnStart()
        {
            base.OnStart();
            _elapsedTime = 0f;
            _spawnTimer = 0f;
            _previousTargetPositions.Clear();
            _controller.UpdateFlyMove();
            SpawnIndicatorsForPlayers();
        }

        public override TaskStatus OnUpdate()
        {
            if (_controller.CurrentStateType != _controller.FlyMoveState)
            {
                _controller.UpdateFlyMove();
            }

            _elapsedTime += Time.deltaTime;
            _spawnTimer += Time.deltaTime;

            if (_elapsedTime >= _airborneDuration)
            {
                return TaskStatus.Success;
            }

            if (_spawnInterval <= 0f)
            {
                return TaskStatus.Running;
            }

            while (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer -= _spawnInterval;
                SpawnIndicatorsForPlayers();
            }

            return TaskStatus.Running;
        }

        private void SpawnIndicatorsForPlayers()
        {
            int targetCount = EnemyFindTarget.CollectValidPlayers(RelayManager.NetworkManagerEx, _targetPlayers);
            if (targetCount == 0)
            {
                return;
            }

            ulong ownerNetworkObjectId = Owner.TryGetComponent(out NetworkObject ownerNetworkObject)
                ? ownerNetworkObject.NetworkObjectId
                : NgoIndicatorController.InvalidSpawnerBossNetworkObjectId;

            for (int i = 0; i < _targetPlayers.Count; i++)
            {
                GameObject targetPlayer = _targetPlayers[i];
                Vector3 indicatorTargetPosition = GetPredictedTargetPosition(targetPlayer);
                GameObject indicatorObject = ResourcesServices.InstantiateByKey(RainIndicatorPath);
                if (indicatorObject.TryGetComponent(out NgoIndicatorController indicatorController) == false)
                {
                    UtilDebug.LogError($"[{nameof(RedDragonAirBombard)}] {nameof(NgoIndicatorController)} is missing.");
                    return;
                }

                GameObject spawnedIndicatorObject = RelayManager.SpawnNetworkObj(indicatorController.gameObject);
                indicatorController = spawnedIndicatorObject.GetComponent<NgoIndicatorController>();
                indicatorController.SetSpawnerBossNetworkObjectId(ownerNetworkObjectId);
                int indicatorDamage = _indicatorDamage;
                indicatorController.SetValue(
                    _indicatorRadius,
                    IndicatorArc,
                    indicatorTargetPosition,
                    _indicatorDuration,
                    () => ApplyIndicatorDamage(indicatorController, indicatorDamage));
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _previousTargetPositions.Clear();
        }

        private Vector3 GetPredictedTargetPosition(GameObject targetPlayer)
        {
            Vector3 currentTargetPosition = GetGroundPosition(targetPlayer.transform.position);
            Vector3 moveDirection = Vector3.zero;

            if (_previousTargetPositions.TryGetValue(targetPlayer, out Vector3 previousTargetPosition))
            {
                moveDirection = currentTargetPosition - previousTargetPosition;
                moveDirection.y = 0f;
            }

            _previousTargetPositions[targetPlayer] = currentTargetPosition;

            if (moveDirection.sqrMagnitude <= Mathf.Epsilon || Random.value < 0.5f)
            {
                return currentTargetPosition;
            }

            Vector3 predictedPosition = currentTargetPosition + moveDirection.normalized * PlayerPredictionDistance;
            return GetGroundPosition(predictedPosition);
        }

        private void ApplyIndicatorDamage(NgoIndicatorController indicatorController, int damage)
        {
            if (_attackRange == null || indicatorController == null)
            {
                return;
            }

            Collider[] targets = Physics.OverlapSphere(
                indicatorController.Position,
                indicatorController.Radius + VerticalCandidatePadding,
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

                float targetPadding = Mathf.Max(target.bounds.extents.x, target.bounds.extents.z);
                if (IsInsideIndicatorCircle(target.bounds.center, indicatorController.Position, indicatorController.Radius + targetPadding) == false)
                {
                    continue;
                }

                if (damage > 0)
                {
                    damageable.OnAttacked(_attackRange, damage);
                }
                else
                {
                    damageable.OnAttacked(_attackRange);
                }

                attackedTargets.Add(damageable);
            }
        }

        private static bool IsInsideIndicatorCircle(Vector3 targetPosition, Vector3 indicatorPosition, float radius)
        {
            Vector2 targetPoint = new Vector2(targetPosition.x, targetPosition.z);
            Vector2 indicatorPoint = new Vector2(indicatorPosition.x, indicatorPosition.z);
            return (targetPoint - indicatorPoint).sqrMagnitude <= radius * radius;
        }

        private static Vector3 GetGroundPosition(Vector3 position)
        {
            Vector3 rayOrigin = position + Vector3.up * 10f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 30f, LayerMask.GetMask("Ground"),
                    QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }

            return position;
        }
    }
}
