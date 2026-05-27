using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GameManagers.RelayManagement;
using UnityEngine;

namespace BehaviourTreeNode.BossGolem.Task
{
    [TaskCategory("CustomNode")]
    public class ThirdBossSubjugationEnding : Action
    {
        [SerializeField] private float _cameraMoveDelay = 2f;
        [SerializeField] private float _uiEndingDelay = 2f;

        private BehaviorTree _tree;
        private RelayManager _relayManager;
        private float _elapsedTime;
        private bool _hasTriggeredCameraMove;

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

        public override void OnStart()
        {
            base.OnStart();
            _tree = Owner.GetComponent<BehaviorTree>();
            _elapsedTime = 0f;
            _hasTriggeredCameraMove = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (_hasTriggeredCameraMove == false)
            {
                _elapsedTime += Time.deltaTime;
                if (_elapsedTime < Mathf.Max(0f, _cameraMoveDelay))
                {
                    return TaskStatus.Running;
                }

                _hasTriggeredCameraMove = true;
                if (RelayManager.NetworkManagerEx.IsHost)
                {
                    RelayManager.NgoRPCCaller.StartThirdBossFrontCameraRpc(_uiEndingDelay);
                }
            }

            if (_tree == null)
            {
                return TaskStatus.Failure;
            }

            _tree.DisableBehavior();
            return TaskStatus.Success;
        }
    }
}
