using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using Controller;
using Data;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using UnityEngine;

namespace BehaviourTreeNode.DarkWizard.Task
{
    [TaskCategory("CustomNode/DarkWizard")]
    public class BossDarkWizardAttack : Action
    {
        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        [SerializeField]private SharedGameObject _targetObject;
        
        public IResourcesServices ResourcesServicesInstance
        {
            get
            {
                if (_resourcesServices == null)
                {
                    _resourcesServices = GetComponent<BossDependencyHub>().ResourcesServices;
                }

                return _resourcesServices;
            }
        }

        private BossController _controller;
        private readonly int _bossAttackAnimHash = BossDarkWizardAnimHash.DarkWizardProjectileAttack;
        private bool _isWaitingFlag = false; // 플래그를 안넣으면 애니메이션 한번쓸떄 AttackCount가 전부 쌓여버림 플래그로 방어

        [SerializeField] private SharedInt _attackCount;
        private int _currentAttackCount = 0;


        public override void OnAwake()
        {
            base.OnAwake();
            _controller = Owner.GetComponent<BossController>();
        }


        public override void OnStart()
        {
            base.OnStart();
            _controller.CurrentStateType = _controller.BaseAttackState; //어택으로 애니메이션 변경
            _currentAttackCount = 0; //공격횟수 초기화
            _isWaitingFlag = false;
        }


        public override TaskStatus OnUpdate()
        {
            //여기서 세번 애니메이션을 실행 해 유도탄을 발사해야함.
            if (_isWaitingFlag)
            {
                // Animator가 갱신되어 진행도가 0으로 떨어지면(완료 상태가 아니게 되면) 플래그 해제
                if (_controller.IsAnimationDone(_bossAttackAnimHash) == false)
                {
                    _isWaitingFlag = false;
                }
                return TaskStatus.Running;
            }
            // 2. 현재 실행 중인 공격 애니메이션이 끝났을 때의 처리
            if (_controller.IsAnimationDone(_bossAttackAnimHash))
            {
                _currentAttackCount++; // 공격 횟수 증가
               // Debug.Log($"현재 공격 완료 횟수 :{_currentAttackCount}");

                // 남은 공격 횟수가 있다면
                if (_currentAttackCount < _attackCount.Value) 
                {
                    // 같은 State라도 강제로 재실행하여 애니메이션을 다시 처음부터 재생
                    _controller.ForceChangeState(_controller.BaseAttackState);

                    _isWaitingFlag = true;
                }
                else // 목표 횟수를 모두 채웠다면
                {
                    // 마지막 애니메이션 재생까지 '완전히' 끝난 시점에 Success 반환
                    return TaskStatus.Success;
                }
            }

            return TaskStatus.Running;
        }


        public override void OnEnd()
        {
            base.OnEnd();
            _controller.CurrentStateType = _controller.BaseIDleState;
        }
    }
}
