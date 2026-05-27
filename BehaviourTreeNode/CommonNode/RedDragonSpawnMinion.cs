using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using Controller.BossState.BossRedDragon;
using Data;
using GameManagers.RelayManagement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Util;
using VFX;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonSpawnMinion : Action
    {
        private const int SpawnIndicatorCount = 10;
        private const string SpawnIndicatorPath = "Prefabs/Enemy/Boss/Indicator/NGODragonSpawnIndicator";
        private const string SpawnMinionClipName = "RedDragonSpawnMinion";
        private const float SpawnIndicatorArc = 360f;
        private const int CandidateSampleCount = 24;
        private const int MaxSpacingRelaxationCount = 4;
        private const float SpacingRelaxationMultiplier = 0.75f;

        [SerializeField] private float _spawnIndicatorRadius = 1.5f;
        [SerializeField] private float _spawnMinRadius = 6f;
        [SerializeField] private float _spawnMaxRadius = 18f;
        [SerializeField] private float _edgePadding = 1.2f;
        [SerializeField] private float _navMeshSampleDistance = 3f;
        [SerializeField, Range(0f, 1f)] private float _minSpawnNormalizedTime = 0.1f;
        [SerializeField, Range(0f, 1f)] private float _maxSpawnNormalizedTime = 1f;
        [SerializeField] private float _indicatorDuration = 0.75f;

        private BossRedDragonController _controller;
        private RelayManager _relayManager;
        private float _spawnAnimLength;
        private float _spawnStartTime;
        private int _nextSpawnIndex;
        private List<Vector3> _scheduledSpawnPositions;
        private List<float> _scheduledSpawnTimes;

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
            _spawnAnimLength = Utill.GetAnimationLength(SpawnMinionClipName, _controller.Anim);
        }

        public override void OnStart()
        {
            base.OnStart();
            _controller.UpdateSpawnMinion();
            PrepareSpawnSchedule();
        }

        public override TaskStatus OnUpdate()
        {
            SpawnScheduledIndicators();

            if (_controller.IsAnimationDone(BossRedDragonAnimHash.RedDragonSpawnMinion))
            {
                SpawnScheduledIndicators(true);
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _scheduledSpawnPositions = null;
            _scheduledSpawnTimes = null;
            _nextSpawnIndex = 0;
            _controller.UpdateIdle();
        }

        private void PrepareSpawnSchedule()
        {
            _spawnStartTime = Time.time;
            _nextSpawnIndex = 0;
            _scheduledSpawnPositions = BuildSpawnPositions(_controller.transform.position);
            _scheduledSpawnTimes = BuildSpawnTimes(_scheduledSpawnPositions.Count);
        }

        private void SpawnScheduledIndicators(bool forceSpawnAll = false)
        {
            if (RelayManager.NetworkManagerEx.IsHost == false || _scheduledSpawnPositions == null || _scheduledSpawnTimes == null)
            {
                return;
            }

            float elapsedTime = Time.time - _spawnStartTime;
            while (_nextSpawnIndex < _scheduledSpawnPositions.Count)
            {
                if (forceSpawnAll == false && elapsedTime < _scheduledSpawnTimes[_nextSpawnIndex])
                {
                    return;
                }

                SpawnIndicator(_scheduledSpawnPositions[_nextSpawnIndex]);
                _nextSpawnIndex++;
            }
        }

        private void SpawnIndicator(Vector3 spawnPosition)
        {
            GameObject indicatorObject = RelayManager.SpawnNetworkObj(SpawnIndicatorPath, position: spawnPosition);
            if (indicatorObject.TryGetComponent(out NgoIndicatorController indicatorController) == false)
            {
                return;
            }

            if (Owner.TryGetComponent(out NetworkObject ownerNetworkObject))
            {
                indicatorController.SetSpawnerBossNetworkObjectId(ownerNetworkObject.NetworkObjectId);
            }

            float indicatorDuration = Mathf.Max(_indicatorDuration, 0.1f);
            indicatorController.SetValue(
                _spawnIndicatorRadius,
                SpawnIndicatorArc,
                spawnPosition,
                indicatorDuration);
        }

        private List<float> BuildSpawnTimes(int spawnCount)
        {
            List<float> spawnTimes = new List<float>(spawnCount);
            if (spawnCount <= 0)
            {
                return spawnTimes;
            }

            float minNormalizedTime = Mathf.Clamp01(_minSpawnNormalizedTime);
            float maxNormalizedTime = Mathf.Max(minNormalizedTime, Mathf.Clamp01(_maxSpawnNormalizedTime));
            for (int i = 0; i < spawnCount; i++)
            {
                float randomNormalizedTime = Random.Range(minNormalizedTime, maxNormalizedTime);
                spawnTimes.Add(randomNormalizedTime * _spawnAnimLength);
            }

            spawnTimes.Sort();
            return spawnTimes;
        }

        private List<Vector3> BuildSpawnPositions(Vector3 centerPosition)
        {
            List<Vector3> spawnPositions = new List<Vector3>(SpawnIndicatorCount);
            float spacing = _spawnIndicatorRadius * 2f;

            for (int i = 0; i < MaxSpacingRelaxationCount && spawnPositions.Count < SpawnIndicatorCount; i++)
            {
                FillSpawnPositions(centerPosition, spacing, spawnPositions);
                spacing *= SpacingRelaxationMultiplier;
            }

            if (spawnPositions.Count < SpawnIndicatorCount)
            {
                FillSpawnPositions(centerPosition, 0f, spawnPositions);
            }

            if (spawnPositions.Count < SpawnIndicatorCount)
            {
                FillFallbackSpawnPositions(centerPosition, spawnPositions);
            }

            return spawnPositions;
        }

        private void FillSpawnPositions(Vector3 centerPosition, float minimumSpacing, List<Vector3> spawnPositions)
        {
            while (spawnPositions.Count < SpawnIndicatorCount)
            {
                bool hasBestCandidate = false;
                Vector3 bestCandidate = default;
                float bestScore = float.MinValue;

                for (int i = 0; i < CandidateSampleCount; i++)
                {
                    if (TryGetCandidatePosition(centerPosition, out Vector3 candidatePosition, out float edgeDistance) == false)
                    {
                        continue;
                    }

                    float nearestDistance = GetNearestDistance(candidatePosition, spawnPositions);
                    if (spawnPositions.Count > 0 && nearestDistance < minimumSpacing)
                    {
                        continue;
                    }

                    float score = nearestDistance + (edgeDistance * 0.25f);
                    if (hasBestCandidate && score <= bestScore)
                    {
                        continue;
                    }

                    hasBestCandidate = true;
                    bestCandidate = candidatePosition;
                    bestScore = score;
                }

                if (hasBestCandidate == false)
                {
                    return;
                }

                spawnPositions.Add(bestCandidate);
            }
        }

        private void FillFallbackSpawnPositions(Vector3 centerPosition, List<Vector3> spawnPositions)
        {
            int fallbackStartIndex = spawnPositions.Count;
            for (int i = fallbackStartIndex; i < SpawnIndicatorCount; i++)
            {
                if (TryGetFallbackPosition(centerPosition, i, out Vector3 fallbackPosition) == false)
                {
                    return;
                }

                spawnPositions.Add(fallbackPosition);
            }
        }

        private bool TryGetCandidatePosition(Vector3 centerPosition, out Vector3 candidatePosition, out float edgeDistance)
        {
            Vector2 randomDirection = Random.insideUnitCircle;
            if (randomDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                randomDirection = Vector2.up;
            }

            randomDirection.Normalize();
            float randomRadius = Random.Range(_spawnMinRadius, _spawnMaxRadius);
            Vector3 samplePosition = centerPosition + new Vector3(randomDirection.x, 0f, randomDirection.y) * randomRadius;

            if (NavMesh.SamplePosition(samplePosition, out NavMeshHit navMeshHit, _navMeshSampleDistance, NavMesh.AllAreas) == false)
            {
                candidatePosition = default;
                edgeDistance = 0f;
                return false;
            }

            if (NavMesh.FindClosestEdge(navMeshHit.position, out NavMeshHit edgeHit, NavMesh.AllAreas) == false)
            {
                candidatePosition = default;
                edgeDistance = 0f;
                return false;
            }

            if (edgeHit.distance < _edgePadding)
            {
                candidatePosition = default;
                edgeDistance = 0f;
                return false;
            }

            candidatePosition = navMeshHit.position;
            edgeDistance = edgeHit.distance;
            return true;
        }

        private bool TryGetFallbackPosition(Vector3 centerPosition, int index, out Vector3 fallbackPosition)
        {
            float normalizedIndex = SpawnIndicatorCount <= 1
                ? 0f
                : (float)index / (SpawnIndicatorCount - 1);
            float radius = Mathf.Lerp(_spawnMinRadius, _spawnMaxRadius, normalizedIndex);
            float angle = 137.5f * index;
            Vector3 samplePosition = centerPosition +
                (Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius);

            if (NavMesh.SamplePosition(
                    samplePosition,
                    out NavMeshHit navMeshHit,
                    _navMeshSampleDistance * 2f,
                    NavMesh.AllAreas))
            {
                fallbackPosition = navMeshHit.position;
                return true;
            }

            if (NavMesh.SamplePosition(
                    centerPosition,
                    out navMeshHit,
                    _navMeshSampleDistance * 2f,
                    NavMesh.AllAreas))
            {
                fallbackPosition = navMeshHit.position;
                return true;
            }

            fallbackPosition = default;
            return false;
        }

        private static float GetNearestDistance(Vector3 candidatePosition, List<Vector3> existingPositions)
        {
            if (existingPositions.Count == 0)
            {
                return float.MaxValue;
            }

            float nearestDistanceSqr = float.MaxValue;
            Vector2 candidateXZ = new Vector2(candidatePosition.x, candidatePosition.z);

            for (int i = 0; i < existingPositions.Count; i++)
            {
                Vector3 existingPosition = existingPositions[i];
                Vector2 existingXZ = new Vector2(existingPosition.x, existingPosition.z);
                float distanceSqr = (candidateXZ - existingXZ).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                }
            }

            return Mathf.Sqrt(nearestDistanceSqr);
        }
    }
}
