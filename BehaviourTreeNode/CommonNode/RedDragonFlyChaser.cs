using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Controller.BossState.BossRedDragon;
using Stats.BaseStats;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonFlyChaser : Action
    {
        [SerializeField] private SharedGameObject _targetObject;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _rotationSpeed = 360f;
        [SerializeField] private float _arriveDistance = 6f;

        private BossRedDragonController _controller;
        private NavMeshAgent _agent;

        public override void OnAwake()
        {
            base.OnAwake();
            _controller = Owner.GetComponent<BossRedDragonController>();
            _agent = Owner.GetComponent<NavMeshAgent>();
        }

        public override void OnStart()
        {
            base.OnStart();
            _controller.UpdateFlyMove();

            // Keep the agent disabled while airborne so XZ chase movement can preserve the jump Y height.
            if (_agent != null && _agent.enabled)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _agent.ResetPath();
                _agent.enabled = false;
            }
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

            if (_targetObject.Value.TryGetComponent(out ITargetable targetable) && targetable.IsTargetable == false)
            {
                return TaskStatus.Failure;
            }

            if (_controller.CurrentStateType != _controller.FlyMoveState)
            {
                _controller.UpdateFlyMove();
            }

            Vector3 targetPosition = _targetObject.Value.transform.position;
            RotateTowardsTarget(targetPosition);

            if (HasArrived(targetPosition))
            {
                return TaskStatus.Success;
            }

            MoveTowardsTarget(targetPosition);
            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
        }

        private void MoveTowardsTarget(Vector3 targetPosition)
        {
            Vector3 currentPosition = Owner.transform.position;
            Vector3 destination = new Vector3(targetPosition.x, currentPosition.y, targetPosition.z);
            Owner.transform.position = Vector3.MoveTowards(
                currentPosition,
                destination,
                _moveSpeed * Time.deltaTime);
        }

        private void RotateTowardsTarget(Vector3 targetPosition)
        {
            Vector3 flatDirection = targetPosition - Owner.transform.position;
            flatDirection.y = 0f;
            if (flatDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized);
            Owner.transform.rotation = Quaternion.RotateTowards(
                Owner.transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }

        private bool HasArrived(Vector3 targetPosition)
        {
            Vector3 currentPosition = Owner.transform.position;
            currentPosition.y = 0f;
            targetPosition.y = 0f;

            float sqrDistance = (targetPosition - currentPosition).sqrMagnitude;
            return sqrDistance <= _arriveDistance * _arriveDistance;
        }
    }
}
