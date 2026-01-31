using System;
using Controller;
using Controller.ControllerStats;
using UnityEngine;

namespace Module.PlayerModule.PlayerClassModule
{
    public class CommonSkillState : IState
    {
        private readonly BaseController _controller;
        private bool _isLocked;

        public int CurrentAnimHash { get; private set; }
        public float CurrentTransitionDuration { get; private set; }

        public CommonSkillState(BaseController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// 외부에서 스킬을 쓰기 전에 이 함수를 호출해 실행할 애니메이션 해쉬값과
        /// 이 애니메이션이 돌아가는동안 고정되는지 여부 를 넣어줌
        /// </summary>
        /// <param name="animHash">애니메이션 해쉬값</param>
        /// <param name="isLocked">애니메이션 고정여부 파라미터</param>
        /// <param name="transitionDuration"></param>
        public void Prepare(int animHash,bool isLocked,float transitionDuration)
        {
            CurrentAnimHash = animHash;
            CurrentTransitionDuration = transitionDuration;
            _isLocked = isLocked;
        }

        public bool LockAnimationChange => _isLocked;

        public event Action UpdateStateEvent;

        public void UpdateState()
        {
            _controller.ChangeAnimIfCurrentIsDone(CurrentAnimHash, _controller.BaseIDleState);
            UpdateStateEvent?.Invoke();
        }
    }
}