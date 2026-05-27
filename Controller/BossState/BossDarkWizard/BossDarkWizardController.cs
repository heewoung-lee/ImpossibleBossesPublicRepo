using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Controller.ControllerStats;
using Controller.ControllerStats.BaseStates;
using Data;
using GameManagers.ResourcesExManagement;
using NetWork.NGO;
using ScenesScripts.SecondBossScene;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Controller.BossState.BossDarkWizard
{
    
    public class HitState : IState
    {
        public HitState(Action hitMethod)
        {
            UpdateStateEvent += hitMethod;
        }
        
        public event Action UpdateStateEvent;
        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }

    public class SlashHitState : IState
    {
        public SlashHitState(Action hitMethod)
        {
            UpdateStateEvent += hitMethod;
        }
        
        public event Action UpdateStateEvent;
        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }
    
    public class CastState : IState
    {
        public CastState(Action castMethod)
        {
            UpdateStateEvent += castMethod;
        }
        
        public event Action UpdateStateEvent;
        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }

    
    
    public class BossDarkWizardController : BossController
    {
        public class BossBossDarkWizardFactory : NgoZenjectFactory<BossDarkWizardController>
        {
            public BossBossDarkWizardFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Character/DarkWizard");
            }
        }
        
        private const float DefaultTransitionTime = 0.05f;
        
        

        private Dictionary<IState, float> _stateDecelerationRatioDict = new Dictionary<IState, float>();
        public override Dictionary<IState, float> StateDecelerationRatioDict => _stateDecelerationRatioDict;
        
        protected override int HashIdle => BossDarkWizardAnimHash.DarkWizardIdle;
        protected override int HashMove => BossDarkWizardAnimHash.DarkWizardFlyForward; // 이동 애니메이션으로 FlyForward 배정
        protected override int HashAttack => BossDarkWizardAnimHash.DarkWizardProjectileAttack;
        protected override int HashDie => BossDarkWizardAnimHash.DarkWizardDie;

        private int _darkWizardHitHash => BossDarkWizardAnimHash.DarkWizardTakeDamage;
        private int _darkWizardCastHash => BossDarkWizardAnimHash.DarkWizardCastSpell;
        private int _darkWizardSlashHitHash => BossDarkWizardAnimHash.DarkWizardSlashAttack;
        
        public override AttackState BaseAttackState => _baseAttackState;
        public override IDleState BaseIDleState => _baseIDleState;
        public override DieState BaseDieState => _baseDieState;
        public override MoveState BaseMoveState => _baseMoveState;

        private AttackState _baseAttackState;
        private IDleState _baseIDleState;
        private DieState _baseDieState;
        private MoveState _baseMoveState;
        
        private SlashHitState _slashHitState;
        private HitState _hitState;
        private CastState _castState;
        

        protected override void AwakeInit()
        {
            base.AwakeInit();
            
            _baseAttackState = new AttackState(UpdateAttack);
            _baseMoveState = new MoveState(UpdateMove);
            _baseDieState = new DieState(UpdateDie);
            _baseIDleState = new IDleState(UpdateIdle);

            _slashHitState = new SlashHitState(UpdateSlashHit);
            _hitState = new HitState(UpdateStateHit);
            _castState = new CastState(UpdateStateCast);
        }

        public void UpdateStateHit() // 현재 상태가 대기 상태 혹은, 움직이는 경우에만 피격판정나도록 설정 전부 넣으면 애니메이션이 번잡해짐
        {
            if (CurrentStateType == _baseIDleState || CurrentStateType == _baseMoveState)
            {
                CurrentStateType = _hitState;
            }
        }

        public void UpdateSlashHit()
        {
            CurrentStateType = _slashHitState;
        }

        public void UpdateStateCast()
        {
            CurrentStateType = _castState;
        }
        
        private void Update()
        {
            //비헤이비어 트리와 관계없이. 피격애니메이션은 해당 컨트롤러에서 돌리는게 제일 효율이 좋을꺼라 생각해서 Update에 기재함. 
            if (CurrentStateType == _hitState) 
            {
                ChangeAnimIfCurrentIsDone(_darkWizardHitHash, BaseIDleState);
            } 
            
        }
        
        public override void UpdateAttack()
        {
            if (CurrentStateType == BaseDieState)
                return;

            CurrentStateType = BaseAttackState;
        }

        public override void UpdateIdle()
        {
            CurrentStateType = BaseIDleState;
        }

        public override void UpdateMove()
        {
            CurrentStateType = BaseMoveState;
        }
   
        public override void UpdateDie()
        {
            if (CurrentStateType == BaseDieState)
            {
                CurrentStateType = BaseDieState;
            }
        }

        protected override void StartInit()
        {
        }

        protected override void AddInitalizeStateDict()
        {
            StateAnimDict.RegisterState(_hitState, ()=> RunAnimation(_darkWizardHitHash,DefaultTransitionTime));
            StateAnimDict.RegisterState(_castState, ()=> RunAnimation(_darkWizardCastHash,DefaultTransitionTime));
            StateAnimDict.RegisterState(_slashHitState, ()=> RunAnimation(_darkWizardSlashHitHash,DefaultTransitionTime));
        }
    }
}
