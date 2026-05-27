using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;
using UnityEngine;

namespace BehaviourTreeNode.CommonNode
{
    [TaskDescription("NavMeshAgent를 즉시 정지시키고 관성을 제거합니다.")]
    [TaskCategory("CustomNode")]
    public class StopAgent : Action
    {
        private NavMeshAgent _agent;

        public override void OnAwake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public override TaskStatus OnUpdate()
        {
            if (_agent != null)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _agent.ResetPath();
            }
            return TaskStatus.Success;
        }
    }
}