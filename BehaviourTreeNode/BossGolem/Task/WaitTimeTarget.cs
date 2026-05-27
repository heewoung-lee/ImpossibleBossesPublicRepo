using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviourTreeNode.BossGolem.Task
{
    [TaskCategory("CustomNode")]
    public class WaitTimeTarget : Conditional
    {
        private float _duration;
        private float _currentTime;
        [SerializeField]private SharedFloat _minSecond;
        [SerializeField]private SharedFloat _maxSecond;

        public override void OnStart()
        {
            base.OnStart();
            _duration = Random.Range(_minSecond.Value, _maxSecond.Value);
            _currentTime = 0f;
        }

        public override TaskStatus OnUpdate()
        {
            _currentTime += Time.deltaTime;
            
            // 지정된 시간이 지나면 Failure를 반환하여 Parallel 노드를 강제로 종료시킴
            if (_currentTime >= _duration)
            {
                return TaskStatus.Failure;
            }
            
            return TaskStatus.Running;
        }
    }
}
