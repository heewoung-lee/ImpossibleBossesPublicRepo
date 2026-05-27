using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using Controller;
using Stats.BaseStats;
using UnityEngine;

namespace BehaviourTreeNode.CommonNode
{
    [TaskDescription("Boss chase node that succeeds only when the target is inside arrive distance.")]
    [TaskCategory("CustomNode")]
    public class BossChaser : NavMeshMovement
    {
        [Header("Targeting")]
        [SerializeField] private SharedGameObject _targetObject;
        
        [Header("Move Animation Speed")]
        [SerializeField] private float _referenceMoveSpeedAtAnimSpeedOne = 3.5f; //보스의 이동속도가 자연스러운 스피드
        [SerializeField] private float _minMoveAnimSpeed = 0.5f;
        [SerializeField] private float _maxMoveAnimSpeed = 1.5f;

        private BossController _controller;
        private IAttackRange _attackRange;

        public override void OnAwake()
        {
            base.OnAwake();
            _controller = GetComponent<BossController>();
            _attackRange = GetComponent(typeof(IAttackRange)) as IAttackRange;
        }

        public override void OnStart()
        {
            base.OnStart();

            if (_controller != null && _controller.CurrentStateType != _controller.BaseMoveState)
            {
                _controller.UpdateMove();
            }

            ApplyMoveAnimationSpeed();
        }

        public override TaskStatus OnUpdate()
        {
            if (_targetObject.Value == null)
            {
                return TaskStatus.Failure;
            }

            if (_targetObject.Value.TryGetComponent(out BaseStats stats) && stats.IsDead)
            {
                return TaskStatus.Failure;
            }

            if (_targetObject.Value.TryGetComponent(out ITargetable targetable) && !targetable.IsTargetable)
            {
                return TaskStatus.Failure;
            }

            Vector3 targetPosition = _targetObject.Value.transform.position;
            Vector3 currentPosition = transform.position;
            targetPosition.y = 0f;
            currentPosition.y = 0f;

            float arriveDistance = m_ArriveDistance.Value;
            bool inRange = (targetPosition - currentPosition).sqrMagnitude <= arriveDistance * arriveDistance;
            bool inViewAngle = IsTargetInsideViewAngle(targetPosition, currentPosition);

            if (inRange && inViewAngle)
            {
                return TaskStatus.Success;
            }

            SetDestination(_targetObject.Value.transform.position);
            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            Stop();

            if (m_NavMeshAgent != null)
            {
                m_NavMeshAgent.velocity = Vector3.zero;
                m_NavMeshAgent.isStopped = true;
            }

            ResetMoveAnimationSpeed();

            base.OnEnd();
        }

        private void ApplyMoveAnimationSpeed()
        {
            if (_controller == null || m_NavMeshAgent == null)
            {
                return;
            }

            float targetAnimSpeed = m_NavMeshAgent.speed / Mathf.Max(_referenceMoveSpeedAtAnimSpeedOne, 0.01f);
            targetAnimSpeed = Mathf.Clamp(targetAnimSpeed, _minMoveAnimSpeed, _maxMoveAnimSpeed);

            _controller.Anim.speed = targetAnimSpeed;
            _controller.SyncCurrentAnimationSpeedOverride(targetAnimSpeed);
        }
        private void ResetMoveAnimationSpeed()
        {
            if (_controller == null)
            {
                return;
            }

            _controller.Anim.speed = 1f;
            _controller.SyncCurrentAnimationSpeedOverride(1f);
        }

        private bool IsTargetInsideViewAngle(Vector3 targetPosition, Vector3 currentPosition)
        {
            if (_attackRange == null)
            {
                return true;
            }

            Vector3 directionToTarget = targetPosition - currentPosition;
            if (directionToTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                return true;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= Mathf.Epsilon)
            {
                return true;
            }

            float angleToTarget = Vector3.Angle(directionToTarget.normalized, forward.normalized);
            return angleToTarget <= _attackRange.ViewAngle * 0.5f;
        }
    }
}
