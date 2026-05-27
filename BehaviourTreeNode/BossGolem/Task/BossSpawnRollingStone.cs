using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Controller.BossState.BossGolem;
using Data;
using GameManagers.NGOPoolManagement;
using GameManagers.RelayManagement;
using Unity.Netcode;
using UnityEngine;
using Util;

namespace BehaviourTreeNode.BossGolem.Task
{
    [TaskCategory("CustomNode/StoneGolem")]
    public class BossSpawnRollingStone : Action
    {
        private const string RollingStonePath = "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoMinionRock";
        private const float SpawnRockSpawnNormalizedTime = 0.44f;

        [SerializeField] private SharedGameObject _targetObject;
        [SerializeField] private float _spawnForwardOffset = 2.5f;
        [SerializeField] private float _spawnVerticalOffset = 1.0f;

        private BossDependencyHub _bossDependencyHub;
        private BossGolemController _controller;
        private bool _hasSpawnedStone;
        private bool _hasFailed;
        private float _previousNormalizedTime;
        private bool _hasSeenSpawnRockState;

        private RelayManager RelayManager
        {
            get
            {
                if (_bossDependencyHub == null)
                {
                    _bossDependencyHub = GetComponent<BossDependencyHub>();
                }

                return _bossDependencyHub.RelayManager;
            }
        }

        private NgoPoolManager NgoPoolManager
        {
            get
            {
                if (_bossDependencyHub == null)
                {
                    _bossDependencyHub = GetComponent<BossDependencyHub>();
                }

                return _bossDependencyHub.NgoPoolManager;
            }
        }

        public override void OnAwake()
        {
            base.OnAwake();
            _controller = Owner.GetComponent<BossGolemController>();
        }

        public override void OnStart()
        {
            base.OnStart();
            _hasSpawnedStone = false;
            _hasFailed = false;
            _previousNormalizedTime = 0f;
            _hasSeenSpawnRockState = false;

            if (RelayManager.NetworkManagerEx.IsHost == false)
            {
                return;
            }

            if (PrepareAnimation() == false || PreparePool() == false)
            {
                _hasFailed = true;
                return;
            }

            _controller.CurrentStateType = _controller.BossSpawnRockState;
        }

        public override TaskStatus OnUpdate()
        {
            if (_hasFailed)
            {
                return TaskStatus.Failure;
            }

            if (RelayManager.NetworkManagerEx.IsHost == false)
            {
                return TaskStatus.Running;
            }

            if (_controller == null)
            {
                return TaskStatus.Failure;
            }

            if (TryGetSpawnRockStateInfo(out AnimatorStateInfo stateInfo) == false)
            {
                return TaskStatus.Running;
            }

            float normalizedTime = stateInfo.normalizedTime;
            float previousNormalizedTime = _hasSeenSpawnRockState
                ? _previousNormalizedTime
                : 0f;

            if (_hasSpawnedStone == false &&
                HasCrossedSpawnThreshold(previousNormalizedTime, normalizedTime))
            {
                _hasSpawnedStone = TrySpawnRollingStone();
                if (_hasSpawnedStone == false)
                {
                    return TaskStatus.Failure;
                }
            }

            _previousNormalizedTime = normalizedTime;
            _hasSeenSpawnRockState = true;

            if (normalizedTime < 1f)
            {
                return TaskStatus.Running;
            }

            return _hasSpawnedStone ? TaskStatus.Success : TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();

            if (_controller != null)
            {
                _controller.CurrentStateType = _controller.BaseIDleState;
            }
        }

        private bool PrepareAnimation()
        {
            if (_controller == null || _controller.Anim == null)
            {
                return false;
            }

            return _controller.Anim.HasState(_controller.AnimLayer, BossGolemAnimHash.GolemSpawnRock);
        }

        private bool TryGetSpawnRockStateInfo(out AnimatorStateInfo stateInfo)
        {
            stateInfo = default;

            if (_controller == null || _controller.Anim == null)
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
            return stateInfo.shortNameHash == BossGolemAnimHash.GolemSpawnRock;
        }

        private bool HasCrossedSpawnThreshold(float previousNormalizedTime, float currentNormalizedTime)
        {
            if (_hasSeenSpawnRockState == false)
            {
                return currentNormalizedTime >= SpawnRockSpawnNormalizedTime;
            }

            return previousNormalizedTime < SpawnRockSpawnNormalizedTime &&
                   currentNormalizedTime >= SpawnRockSpawnNormalizedTime;
        }

        private bool PreparePool()
        {
            if (NgoPoolManager == null)
            {
                return false;
            }

            if (NgoPoolManager.TryGetPool(RollingStonePath, out _) == false)
            {
                NgoPoolManager.EnsurePoolRegistered(RollingStonePath);

                if (NgoPoolManager.TryGetPool(RollingStonePath, out _) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool TrySpawnRollingStone()
        {
            Vector3 spawnPosition = Owner.transform.position + Owner.transform.forward * _spawnForwardOffset +
                                    Vector3.up * _spawnVerticalOffset;
            Vector3 moveDirection = GetMoveDirection(spawnPosition);
            Quaternion spawnRotation = Quaternion.LookRotation(moveDirection);
            NetworkObject rollingStoneNetworkObject = NgoPoolManager.GetPooledObject(RollingStonePath);

            if (rollingStoneNetworkObject == null)
            {
                return false;
            }

            rollingStoneNetworkObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            GameObject rollingStoneObject = RelayManager.SpawnNetworkObj(
                rollingStoneNetworkObject.gameObject,
                position: spawnPosition);

            if (rollingStoneObject == null)
            {
                return false;
            }

            if (rollingStoneObject.TryGetComponent(out StoneGolemRollingRockController rollingRockController) == false)
            {
                UtilDebug.LogError($"[{nameof(BossSpawnRollingStone)}] {nameof(StoneGolemRollingRockController)} is missing.");
                return false;
            }

            rollingRockController.InitializeRolling(moveDirection);
            return true;
        }

        private Vector3 GetMoveDirection(Vector3 spawnPosition)
        {
            if (_targetObject?.Value != null)
            {
                Vector3 towardTarget = _targetObject.Value.transform.position - spawnPosition;
                towardTarget.y = 0f;
                if (towardTarget.sqrMagnitude > Mathf.Epsilon)
                {
                    return towardTarget.normalized;
                }
            }

            Vector3 forward = Owner.transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > Mathf.Epsilon ? forward.normalized : Vector3.forward;
        }
    }
}
