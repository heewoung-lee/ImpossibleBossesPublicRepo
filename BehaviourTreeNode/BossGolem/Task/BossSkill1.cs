using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Character.Skill.AllofSkills.BossMonster.StoneGolem;
using Controller.BossState.BossGolem;
using GameManagers.RelayManagement;
using GameManagers.VFXManagement;
using NetWork;
using Unity.Netcode;
using UnityEngine;
using Util;

namespace BehaviourTreeNode.BossGolem.Task
{
    [TaskCategory("CustomNode/StoneGolem")]
    public class BossSkill1 : Action
    {
        private const string Skill1LaunchRockPath = "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoRockToThrowSky";
        private const string Skill1IndicatorPath = "Prefabs/Enemy/Boss/Indicator/Boss_Skill1_Indicator";
        private const string Skill1DropAttackPath = "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoBossSkill1Attack";
        private const float Skill1AnimStopThreshold = 0.02f;
        private const float Skill1StartAnimSpeed = 1f;
        private const float ProjectileTravelDistance = 8f;
        private const float ProjectileThrowSpeed = 20f;
        private const float MinProjectileConeAngle = 10f;
        private const float MaxProjectileConeAngle = 42f;
        private const float IndicatorSpawnInterval = 0.3f;
        private const float IndicatorDuration = 1f;
        private const float DropAttackHeight = 12f;
        private const float PlayerPredictionDistance = 3f;
        private const float DropAttackFallSpeed = 30f;

        private BossDependencyHub _bossDependencyHub;
        private RelayManager _relayManager;
        private BossGolemController _controller;
        private NGOBossNetworkController _networkController;
        private float _animLength;
        private double _projectileSpawnStartTime;
        private int _spawnedProjectileCount;
        private float[] _projectileSpawnSchedule;
        private bool _hasStartedIndependentIndicatorSequence;
        private readonly List<GameObject> _targetPlayers = new List<GameObject>();
        private readonly Dictionary<GameObject, Vector3> _previousTargetPositions = new Dictionary<GameObject, Vector3>();

        [SerializeField] private int _projectileCount = 20;
        [SerializeField, Min(0f)] private float _projectileSpawnDuration = 1f;
        [SerializeField] private SharedInt _damage;

        private BossDependencyHub BossDependencyHub
        {
            get
            {
                if (_bossDependencyHub == null)
                {
                    _bossDependencyHub = GetComponent<BossDependencyHub>();
                }

                return _bossDependencyHub;
            }
        }

        private RelayManager RelayManager
        {
            get
            {
                if (_relayManager == null)
                {
                    _relayManager = BossDependencyHub.RelayManager;
                }

                return _relayManager;
            }
        }

        private IVFXManagerServices VfxManagerServices => BossDependencyHub.VfxManagerServices;

        public override void OnAwake()
        {
            base.OnAwake();
            _controller = Owner.GetComponent<BossGolemController>();
            _networkController = Owner.GetComponent<NGOBossNetworkController>();
            _animLength = Utill.GetAnimationLength("Anim_Hit", _controller.Anim);
        }

        public override void OnStart()
        {
            base.OnStart();

            if (_controller.TryGetAttackTypePreTime(_controller.BossSkill1State, out float decelerationRatio) == false)
            {
                return;
            }

            float projectileSpawnDuration = Mathf.Max(0f, _projectileSpawnDuration);
            _projectileCount = Mathf.Max(0, _projectileCount);
            _controller.CurrentStateType = _controller.BossSkill1State;
            _spawnedProjectileCount = 0;
            _hasStartedIndependentIndicatorSequence = false;
            _targetPlayers.Clear();
            _previousTargetPositions.Clear();
            _projectileSpawnStartTime = RelayManager.NetworkManagerEx.ServerTime.Time;
            _projectileSpawnSchedule = BuildProjectileSpawnSchedule(projectileSpawnDuration, _projectileCount);

            NetworkAnimationInfo animInfo = new NetworkAnimationInfo(
                _animLength,
                decelerationRatio,
                Skill1AnimStopThreshold,
                projectileSpawnDuration,
                RelayManager.NetworkManagerEx.ServerTime.Time,
                Skill1StartAnimSpeed);
            _networkController.StartAnimChangedRpc(animInfo);
        }

        public override TaskStatus OnUpdate()
        {
            SpawnProjectiles();

            bool finishedSkillSequence = _networkController.FinishAttack &&
                                         _spawnedProjectileCount >= _projectileCount;
            if (finishedSkillSequence == false)
            {
                return TaskStatus.Running;
            }

            StartIndependentIndicatorSequence();
            return TaskStatus.Success;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _projectileSpawnSchedule = null;
            _spawnedProjectileCount = 0;
            _hasStartedIndependentIndicatorSequence = false;
            _targetPlayers.Clear();
            _previousTargetPositions.Clear();
            _controller.CurrentStateType = _controller.BaseIDleState;
        }

        private void SpawnProjectiles()
        {
            if (_projectileSpawnSchedule == null || _spawnedProjectileCount >= _projectileCount)
            {
                return;
            }

            double elapsedTime = RelayManager.NetworkManagerEx.ServerTime.Time - _projectileSpawnStartTime;
            while (_spawnedProjectileCount < _projectileCount &&
                   elapsedTime >= _projectileSpawnSchedule[_spawnedProjectileCount])
            {
                SpawnSingleProjectile();
                _spawnedProjectileCount++;
            }
        }

        private void SpawnSingleProjectile()
        {
            ulong ownerNetworkObjectId = Owner.TryGetComponent(out NetworkObject ownerNetworkObject)
                ? ownerNetworkObject.NetworkObjectId
                : ulong.MaxValue;
            Vector3 projectileSpawnPosition = GetProjectileSpawnPosition();
            Vector3 projectileTargetPosition = GetRandomSkyTargetPosition(projectileSpawnPosition);
            float projectileFlightDuration = GetProjectileFlightDuration(projectileSpawnPosition, projectileTargetPosition);

            NetworkParams skill1AttackParam = new NetworkParams(
                argPosVector3: projectileTargetPosition,
                argBoolean: false,
                argUlong: ownerNetworkObjectId);

            GameObject spawnedProjectile = RelayManager.SpawnNetworkObj(
                Skill1LaunchRockPath,
                position: projectileSpawnPosition);

            if (spawnedProjectile.TryGetComponent(out NgoRockToThrowSkyInitialize throwInitialize) == false)
            {
                throw new MissingComponentException(
                    $"[{nameof(BossSkill1)}] {nameof(NgoRockToThrowSkyInitialize)} is missing.");
            }

            throwInitialize.InitializeVfxClientRpc(projectileFlightDuration, skill1AttackParam);
        }

        private Vector3 GetProjectileSpawnPosition()
        {
            Collider ownerCollider = Owner.GetComponent<Collider>();
            if (ownerCollider == null)
            {
                return Owner.transform.position;
            }

            Vector3 ownerPosition = Owner.transform.position;
            return new Vector3(ownerPosition.x, ownerCollider.bounds.max.y, ownerPosition.z);
        }

        private Vector3 GetRandomSkyTargetPosition(Vector3 projectileSpawnPosition)
        {
            float randomYaw = Random.Range(0f, 360f);
            float randomConeAngle = Random.Range(MinProjectileConeAngle, MaxProjectileConeAngle);
            Quaternion coneRotation =
                Quaternion.AngleAxis(randomYaw, Owner.transform.up) *
                Quaternion.AngleAxis(randomConeAngle, Owner.transform.right);
            Vector3 projectileDirection = coneRotation * Owner.transform.up;
            return projectileSpawnPosition + projectileDirection.normalized * ProjectileTravelDistance;
        }

        private void StartIndependentIndicatorSequence()
        {
            if (_hasStartedIndependentIndicatorSequence || _projectileCount <= 0)
            {
                return;
            }

            _hasStartedIndependentIndicatorSequence = true;
            _controller.StartCoroutine(IndependentIndicatorSequenceRoutine(_projectileCount, _damage.Value));
        }

        private IEnumerator IndependentIndicatorSequenceRoutine(int totalIndicatorCount, int attackDamage)
        {
            for (int i = 0; i < totalIndicatorCount; i++)
            {
                SpawnIndicatorsForPlayers(attackDamage);

                if (i < totalIndicatorCount - 1)
                {
                    yield return new WaitForSeconds(IndicatorSpawnInterval);
                }
            }
        }

        private void SpawnIndicatorsForPlayers(int attackDamage)
        {
            int targetCount = EnemyFindTarget.CollectValidPlayers(RelayManager.NetworkManagerEx, _targetPlayers);
            if (targetCount == 0)
            {
                return;
            }

            for (int i = 0; i < targetCount; i++)
            {
                Vector3 indicatorTargetPosition = GetPredictedTargetPosition(_targetPlayers[i]);
                SpawnSkill1Indicator(indicatorTargetPosition, attackDamage);
                _controller.StartCoroutine(SpawnDropAttackAfterDelayRoutine(indicatorTargetPosition, attackDamage));
            }
        }

        private IEnumerator SpawnDropAttackAfterDelayRoutine(Vector3 targetPosition, int attackDamage)
        {
            float dropAttackDuration = GetDropAttackDuration();
            float dropAttackSpawnDelay = Mathf.Max(IndicatorDuration - dropAttackDuration, 0f);
            yield return new WaitForSeconds(dropAttackSpawnDelay);
            SpawnDropAttack(targetPosition, attackDamage);
        }

        private void SpawnSkill1Indicator(Vector3 indicatorTargetPosition, int attackDamage)
        {
            ulong ownerNetworkObjectId = Owner.TryGetComponent(out NetworkObject ownerNetworkObject)
                ? ownerNetworkObject.NetworkObjectId
                : ulong.MaxValue;

            NetworkParams skill1IndicatorParam = new NetworkParams(
                argFloat: IndicatorDuration,
                argPosVector3: indicatorTargetPosition,
                argInteger: attackDamage,
                argBoolean: false,
                argUlong: ownerNetworkObjectId);
            RelayManager.NgoRPCCaller.SpawnLocalObject(indicatorTargetPosition, Skill1IndicatorPath, skill1IndicatorParam);
        }

        private void SpawnDropAttack(Vector3 targetPosition, int attackDamage)
        {
            ulong ownerNetworkObjectId = Owner.TryGetComponent(out NetworkObject ownerNetworkObject)
                ? ownerNetworkObject.NetworkObjectId
                : ulong.MaxValue;

            Vector3 spawnPosition = targetPosition + Vector3.up * DropAttackHeight;
            float dropAttackDuration = GetDropAttackDuration();
            NetworkParams dropAttackParam = new NetworkParams(
                argPosVector3: targetPosition,
                argInteger: attackDamage,
                argBoolean: true,
                argUlong: ownerNetworkObjectId);

            VfxManagerServices.InstantiateParticleInArea(
                Skill1DropAttackPath,
                spawnPosition,
                dropAttackDuration,
                networkParams: dropAttackParam);
        }

        private static float GetDropAttackDuration()
        {
            return Mathf.Max(DropAttackHeight / DropAttackFallSpeed, 0.1f);
        }

        private Vector3 GetPredictedTargetPosition(GameObject targetObject)
        {
            if (targetObject == null)
            {
                return GetProjectileSpawnPosition();
            }

            Vector3 currentTargetPosition = GetGroundPosition(targetObject.transform.position);
            Vector3 moveDirection = Vector3.zero;

            if (_previousTargetPositions.TryGetValue(targetObject, out Vector3 previousTargetPosition))
            {
                moveDirection = currentTargetPosition - previousTargetPosition;
                moveDirection.y = 0f;
            }

            _previousTargetPositions[targetObject] = currentTargetPosition;

            if (moveDirection.sqrMagnitude <= Mathf.Epsilon || Random.value < 0.5f)
            {
                return currentTargetPosition;
            }

            Vector3 predictedPosition = currentTargetPosition + moveDirection.normalized * PlayerPredictionDistance;
            return GetGroundPosition(predictedPosition);
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

        private float GetProjectileFlightDuration(Vector3 projectileSpawnPosition, Vector3 projectileTargetPosition)
        {
            float projectileDistance = Vector3.Distance(projectileSpawnPosition, projectileTargetPosition);
            return Mathf.Max(projectileDistance / ProjectileThrowSpeed, 0.1f);
        }

        private static float[] BuildProjectileSpawnSchedule(float spawnDuration, int projectileCount)
        {
            float[] projectileSpawnSchedule = new float[projectileCount];
            if (projectileCount <= 1 || spawnDuration <= 0f)
            {
                return projectileSpawnSchedule;
            }

            float[] randomGapWeights = new float[projectileCount - 1];
            float totalRandomWeight = 0f;
            for (int i = 0; i < randomGapWeights.Length; i++)
            {
                randomGapWeights[i] = Random.Range(0.2f, 1f);
                totalRandomWeight += randomGapWeights[i];
            }

            float accumulatedTime = 0f;
            for (int i = 1; i < projectileSpawnSchedule.Length; i++)
            {
                accumulatedTime += spawnDuration * (randomGapWeights[i - 1] / totalRandomWeight);
                projectileSpawnSchedule[i] = accumulatedTime;
            }

            projectileSpawnSchedule[projectileSpawnSchedule.Length - 1] = spawnDuration;
            return projectileSpawnSchedule;
        }
    }
}
