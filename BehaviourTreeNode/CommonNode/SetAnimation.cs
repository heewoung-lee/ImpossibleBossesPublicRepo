using BehaviorDesigner.Runtime.Tasks;
using Controller;
using Controller.ControllerStats;
using UnityEngine;
using Util;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace BehaviourTreeNode.CommonNode
{
    enum State
    {
        IDle,
        Move,
        Attack,
        Dead
    }
    
    
    [TaskDescription("보스의 애니메이션 상태를 전환합니다.")]
    [TaskCategory("CustomNode")]
    public class SetBaseAnimation : Action
    {
        private BossController _controller;
        [SerializeField] private State _eState;

        private IState _state;

        public override void OnAwake()
        {
            _controller = GetComponent<BossController>();
        }

        public override void OnStart()
        {
            _state = GetState(_eState);
        }
        
        public override TaskStatus OnUpdate()
        {
            //현재 애니메이션이 지금 애니메이션 과 다를 경우에만 바꿈
            if (_controller != null && _controller.CurrentStateType != _state)
            {
                _controller.CurrentStateType = _state;
            }
            return TaskStatus.Success; // 즉시 성공하여 다음 Seek으로 넘어감
        }


        private IState GetState(State state)
        {
            switch (state)
            {
                case State.IDle:
                    return _controller.BaseIDleState;
                case State.Move:
                    return _controller.BaseMoveState;
                case State.Attack:
                    return _controller.BaseAttackState;
                case State.Dead:
                    return _controller.BaseAttackState;
            }
            
            UtilDebug.LogError($"{gameObject.name}에 할당된 {state} 가 없습니다.");
            return null;
        }
    }
}