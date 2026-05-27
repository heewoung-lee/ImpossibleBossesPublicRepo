using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Controller.BossState.BossRedDragon;
using Data;
using NetWork.BossRedDragon_NGO;
using UnityEngine;
using UnityEngine.AI;
using Util;
using VFX;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonLanding : Action
    {
        private const string LandingClipName = "RedDragonLanding";
        private const float GroundTouchIndicatorProgress = 1f;
        private const float SlowLandingAnimNormalizedRange = 0.10f;

        [SerializeField] private SharedProjector _landingIndicator;
        [SerializeField, Range(1.1f, 2f)] private float _fallAccelerationPower = 1.45f;
        [SerializeField] private float _landingNavMeshSampleDistance = 5f;

        private BossRedDragonController _controller;
        private NGOBossRedDragonNetworkController _networkController;
        private NavMeshAgent _agent;
        private NgoIndicatorController _indicatorController;
        private Vector3 _landingTargetPosition;
        private float _fallStartY;
        private float _landingAnimLength;
        private float _defaultAnimSpeed;
        private float _airborneAnimSpeed;
        private bool _hasReachedGround;

        public override void OnAwake()
        {
            base.OnAwake();
            _controller = Owner.GetComponent<BossRedDragonController>();
            _networkController = Owner.GetComponent<NGOBossRedDragonNetworkController>();
            _agent = Owner.GetComponent<NavMeshAgent>();
            _landingAnimLength = Utill.GetAnimationLength(LandingClipName, _controller.Anim);
        }

        public override void OnStart()
        {
            base.OnStart();
            _defaultAnimSpeed = _controller.Anim.speed;
            _airborneAnimSpeed = _defaultAnimSpeed;
            _hasReachedGround = false;
            _indicatorController = _landingIndicator.Value as NgoIndicatorController;
            if (_indicatorController == null)
            {
                return;
            }

            _landingTargetPosition = _indicatorController.Position;
            _landingTargetPosition.y = ResolveLandingGroundY(_landingTargetPosition);
            _fallStartY = Owner.transform.position.y;
            _airborneAnimSpeed = CalculateAirborneAnimSpeed();

            // Landing starts immediately, but its first 10% is slowed down so the dragon can stay
            // airborne until the indicator reaches 50%.
            _controller.UpdateLandingState();
        }

        public override TaskStatus OnUpdate()
        {
            if (_indicatorController == null)
            {
                return TaskStatus.Failure;
            }

            float indicatorProgress = Mathf.Clamp01(_indicatorController.NormalizedProgress);
            if (_hasReachedGround == false)
            {
                UpdateAirborneLanding(indicatorProgress);
            }

            return _controller.IsAnimationDone(BossRedDragonAnimHash.RedDragonLanding)
                ? TaskStatus.Success
                : TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _controller.Anim.speed = _defaultAnimSpeed;
        }

        private void UpdateAirborneLanding(float indicatorProgress)
        {
            LockLandingXZ();

            if (indicatorProgress < GroundTouchIndicatorProgress)
            {
                _controller.Anim.speed = _airborneAnimSpeed;
                UpdateFallingY(indicatorProgress);
                return;
            }

            _hasReachedGround = true;
            _controller.Anim.speed = _defaultAnimSpeed;
            SnapToGroundHeight();
            RestoreNavMeshAgent();
        }

        private void UpdateFallingY(float indicatorProgress)
        {
            float normalizedFallProgress = Mathf.Clamp01(indicatorProgress / GroundTouchIndicatorProgress);
            float acceleratedFallProgress = Mathf.Pow(normalizedFallProgress, _fallAccelerationPower);

            Vector3 ownerPosition = Owner.transform.position;
            ownerPosition.y = Mathf.Lerp(_fallStartY, _landingTargetPosition.y, acceleratedFallProgress);
            Owner.transform.position = ownerPosition;
        }

        private void LockLandingXZ()
        {
            Vector3 ownerPosition = Owner.transform.position;
            ownerPosition.x = _landingTargetPosition.x;
            ownerPosition.z = _landingTargetPosition.z;
            Owner.transform.position = ownerPosition;
        }

        private void SnapToGroundHeight()
        {
            Vector3 ownerPosition = Owner.transform.position;
            ownerPosition.y = _landingTargetPosition.y;
            Owner.transform.position = ownerPosition;
        }

        private float CalculateAirborneAnimSpeed()
        {
            if (_indicatorController == null || _indicatorController.DurationTime <= Mathf.Epsilon || _landingAnimLength <= Mathf.Epsilon)
            {
                return _defaultAnimSpeed;
            }

            float targetAirborneDuration = _indicatorController.DurationTime * GroundTouchIndicatorProgress;
            float clipAirborneDuration = _landingAnimLength * SlowLandingAnimNormalizedRange;
            return Mathf.Max(clipAirborneDuration / targetAirborneDuration, 0.01f);
        }

        private float ResolveLandingGroundY(Vector3 landingPosition)
        {
            if (NavMesh.SamplePosition(landingPosition, out NavMeshHit navHit, _landingNavMeshSampleDistance, NavMesh.AllAreas))
            {
                return navHit.position.y;
            }

            return _controller.AirborneGroundY;
        }

        private void RestoreNavMeshAgent()
        {
            if (_networkController != null)
            {
                _networkController.SetAirborneState(false);
                return;
            }

            if (_agent == null || _agent.enabled)
            {
                return;
            }

            // Landing hands movement ownership back to the ground navigation layer.
            _agent.enabled = true;
            _agent.Warp(Owner.transform.position);
            _agent.velocity = Vector3.zero;
            _agent.ResetPath();
            _agent.isStopped = true;
        }
    }
}
