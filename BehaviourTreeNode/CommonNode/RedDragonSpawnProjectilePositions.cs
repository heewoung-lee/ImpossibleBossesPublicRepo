using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using Character.Skill.AllofSkills.BossMonster.RedDragon;
using Controller.BossState.BossRedDragon;
using Data;
using GameManagers.RelayManagement;
using NetWork.BossRedDragon_NGO;
using Stats.BossStats;
using UnityEngine;
using Util;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonSpawnProjectilePositions : Action
    {
        private const string ProjectilePath = "Prefabs/Enemy/Boss/AttackPattern/RedDragon/RedDragonProjectileVFX";
        private const float SpawnStartNormalizedTime = 0.33f;
        private const float SpawnEndNormalizedTime = 0.47f;

        [SerializeField] private int _minProjectileCountPerPosition = 4;
        [SerializeField] private int _maxProjectileCountPerPosition = 8;
        [SerializeField] private int _projectileDamage = 10;
        [SerializeField] private float _baseScatterHoldTime = 0.2f;
        [SerializeField] private float _minScatterDelay = 0.1f;
        [SerializeField] private float _maxScatterDelay = 0.6f;
        [SerializeField] private float _minScatterAngle = -90f;
        [SerializeField] private float _maxScatterAngle = 90f;

        private RelayManager _relayManager;
        private BossRedDragonController _controller;
        private BossRedDragonStats _attackerStats;
        private readonly List<Transform> _projectilePositions = new List<Transform>();
        private int _nextSpawnIndex;
        private float _previousNormalizedTime;
        private bool _hasSeenProjectileAttackState;

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
            _attackerStats = Owner.GetComponent<BossRedDragonStats>();
            CacheProjectilePositions();
        }

        public override void OnStart()
        {
            base.OnStart();
            _nextSpawnIndex = 0;
            _previousNormalizedTime = 0f;
            _hasSeenProjectileAttackState = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (_projectilePositions.Count == 0)
            {
                return TaskStatus.Failure;
            }

            if (_nextSpawnIndex >= _projectilePositions.Count)
            {
                return TaskStatus.Success;
            }

            if (TryGetProjectileAttackStateInfo(out AnimatorStateInfo stateInfo) == false)
            {
                return TaskStatus.Running;
            }

            float normalizedTime = Mathf.Clamp01(stateInfo.normalizedTime);
            float previousNormalizedTime = _hasSeenProjectileAttackState
                ? _previousNormalizedTime
                : 0f;

            while (_nextSpawnIndex < _projectilePositions.Count &&
                   HasCrossedSpawnThreshold(previousNormalizedTime, normalizedTime, _nextSpawnIndex))
            {
                SpawnProjectilesAt(_projectilePositions[_nextSpawnIndex]);
                _nextSpawnIndex++;
            }

            _previousNormalizedTime = normalizedTime;
            _hasSeenProjectileAttackState = true;

            return _nextSpawnIndex >= _projectilePositions.Count
                ? TaskStatus.Success
                : TaskStatus.Running;
        }

        private void CacheProjectilePositions()
        {
            _projectilePositions.Clear();

            RedDragonProjectilePositionMarker marker = Owner.transform.GetComponentInChildren<RedDragonProjectilePositionMarker>();
            if (marker == null)
            {
                UtilDebug.LogError($"[{nameof(RedDragonProjectilePositionMarker)}] not found.");
                return;
            }

            Transform projectilePositionsRoot = marker.transform;

            for (int i = 0; i < projectilePositionsRoot.childCount; i++)
            {
                Transform child = projectilePositionsRoot.GetChild(i);
                if (child != null)
                {
                    _projectilePositions.Add(child);
                }
            }
        }

        private bool TryGetProjectileAttackStateInfo(out AnimatorStateInfo stateInfo)
        {
            stateInfo = default;
            if (_controller == null)
            {
                return false;
            }

            Animator animator = _controller.Anim;
            int animLayer = _controller.AnimLayer;

            if (animator.IsInTransition(animLayer))
            {
                return false;
            }

            stateInfo = animator.GetCurrentAnimatorStateInfo(animLayer);
            return stateInfo.shortNameHash == BossRedDragonAnimHash.RedDragonProjectileAttack;
        }

        private float GetSpawnNormalizedTime(int spawnIndex)
        {
            if (_projectilePositions.Count <= 1)
            {
                return SpawnStartNormalizedTime;
            }

            float progress = spawnIndex / (float)(_projectilePositions.Count - 1);
            return Mathf.Lerp(SpawnStartNormalizedTime, SpawnEndNormalizedTime, progress);
        }

        private bool HasCrossedSpawnThreshold(float previousNormalizedTime, float currentNormalizedTime, int spawnIndex)
        {
            float spawnNormalizedTime = GetSpawnNormalizedTime(spawnIndex);
            if (_hasSeenProjectileAttackState == false)
            {
                return currentNormalizedTime >= spawnNormalizedTime;
            }

            return previousNormalizedTime < spawnNormalizedTime &&
                   currentNormalizedTime >= spawnNormalizedTime;
        }

        private void SpawnProjectilesAt(Transform spawnPoint)
        {
            int spawnCount = Random.Range(
                Mathf.Min(_minProjectileCountPerPosition, _maxProjectileCountPerPosition),
                Mathf.Max(_minProjectileCountPerPosition, _maxProjectileCountPerPosition) + 1);

            Quaternion projectileRotation = GetProjectileRotation();

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject spawnedProjectile = RelayManager.SpawnNetworkObj(
                    ProjectilePath,
                    position: spawnPoint.position);

                if (spawnedProjectile != null)
                {
                    float scatterDelay = Random.Range(
                        Mathf.Min(_minScatterDelay, _maxScatterDelay),
                        Mathf.Max(_minScatterDelay, _maxScatterDelay));
                    float totalScatterDelay = Mathf.Max(0f, _baseScatterHoldTime) + scatterDelay;

                    float scatterAngle = Random.Range(
                        Mathf.Min(_minScatterAngle, _maxScatterAngle),
                        Mathf.Max(_minScatterAngle, _maxScatterAngle));

                    if (spawnedProjectile.TryGetComponent(out NgoRedDragonProjectileLifetimeBehaviour scatterController))
                    {
                        scatterController.StartScatter(
                            spawnPoint.position,
                            projectileRotation,
                            totalScatterDelay,
                            scatterAngle,
                            _attackerStats,
                            _projectileDamage);
                    }
                    else
                    {
                        spawnedProjectile.transform.rotation = projectileRotation;
                    }
                }
            }
        }

        private Quaternion GetProjectileRotation()
        {
            if (_controller == null)
            {
                return Quaternion.identity;
            }

            Vector3 forward = _controller.transform.forward;
            if (forward.sqrMagnitude <= Mathf.Epsilon)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(forward, Vector3.up);
        }
    }
}
