using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using Controller.BossState.BossRedDragon;
using Data;
using GameManagers.RelayManagement;
using NetWork;
using NetWork.BossRedDragon_NGO;
using UnityEngine;
using UnityEngine.AI;
using Util;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonJump : Action
    {
        private const float JumpAnimationSpeed = 0.4f;
        private const float LiftStartNormalizedTime = 0.37f;

        [SerializeField] private float _liftHeight = 6f;

        private BossRedDragonController _controller;
        private NGOBossRedDragonNetworkController _networkController;
        private RelayManager _relayManager;
        private NavMeshAgent _agent;
        private float _startY;
        private float _animLength;
        private bool _hasStartedLift;

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
            _networkController = Owner.GetComponent<NGOBossRedDragonNetworkController>();
            _agent = Owner.GetComponent<NavMeshAgent>();
            _animLength = Utill.GetAnimationLength("RedDragonJump", _controller.Anim);
        }

        public override void OnStart()
        {
            base.OnStart();
            _startY = Owner.transform.position.y;
            _controller.AirborneGroundY = _startY;
            _hasStartedLift = false;
            _controller.UpdateJumpState();
            _controller.Anim.speed = JumpAnimationSpeed;
            StartAnimationSpeedChanged();
            _networkController?.SetAirborneState(true);

            // Airborne movement owns Y directly, so the NavMeshAgent must be disabled
            // when the jump starts or it will snap the dragon back to the ground plane.
            if (_networkController == null && _agent != null && _agent.enabled)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _agent.ResetPath();
                _agent.enabled = false;
            }
        }

        public override TaskStatus OnUpdate()
        {
            AnimatorStateInfo stateInfo = _controller.Anim.GetCurrentAnimatorStateInfo(_controller.AnimLayer);
            if (stateInfo.shortNameHash != BossRedDragonAnimHash.RedDragonJump)
            {
                return TaskStatus.Running;
            }

            float normalizedTime = Mathf.Clamp01(stateInfo.normalizedTime);
            if (normalizedTime >= LiftStartNormalizedTime)
            {
                _hasStartedLift = true;
                float liftProgress = Mathf.InverseLerp(LiftStartNormalizedTime, 1f, normalizedTime);
                Vector3 currentPosition = Owner.transform.position;
                currentPosition.y = Mathf.Lerp(_startY, _startY + _liftHeight, liftProgress);
                Owner.transform.position = currentPosition;
            }

            if (normalizedTime >= 1f && _controller.Anim.IsInTransition(_controller.AnimLayer) == false)
            {
                if (_hasStartedLift)
                {
                    Vector3 finalPosition = Owner.transform.position;
                    finalPosition.y = _startY + _liftHeight;
                    Owner.transform.position = finalPosition;
                }

                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _controller.Anim.speed = 1f;
        }

        private void StartAnimationSpeedChanged()
        {
            if (_networkController == null)
            {
                return;
            }

            NetworkAnimationInfo animInfo = new NetworkAnimationInfo(
                _animLength,
                0f,
                0f,
                0f,
                RelayManager.NetworkManagerEx.ServerTime.Time,
                JumpAnimationSpeed);

            _networkController.StartJumpAnimChangedRpc(animInfo);
        }
    }
}
