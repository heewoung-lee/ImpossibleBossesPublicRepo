using System.Collections.Generic;
using Controller.BossState.BossDarkWizard;
using Controller.ControllerStats;
using Controller.ControllerStats.BaseStates;
using Data;
using GameManagers.ResourcesExManagement;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Controller.BossState.BossRedDragon
{
    public class RedDragonTailAttackState : IState
    {
        public RedDragonTailAttackState(System.Action attackMethod)
        {
            UpdateStateEvent += attackMethod;
        }

        public event System.Action UpdateStateEvent;

        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }
    
    public class RedDragonJumpState : IState
    {
        public RedDragonJumpState(System.Action attackMethod)
        {
            UpdateStateEvent += attackMethod;
        }

        public event System.Action UpdateStateEvent;

        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }

    public class RedDragonFlyMoveState : IState
    {
        public RedDragonFlyMoveState(System.Action moveMethod)
        {
            UpdateStateEvent += moveMethod;
        }

        public event System.Action UpdateStateEvent;

        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }

    public class RedDragonLandingState : IState
    {
        public RedDragonLandingState(System.Action landingMethod)
        {
            UpdateStateEvent += landingMethod;
        }

        public event System.Action UpdateStateEvent;

        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }

    public class RedDragonBreathStartState : IState
    {
        public RedDragonBreathStartState(System.Action startMethod)
        {
            UpdateStateEvent += startMethod;
        }

        public event System.Action UpdateStateEvent;

        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }

    public class RedDragonBreathLoopState : IState
    {
        public RedDragonBreathLoopState(System.Action loopMethod)
        {
            UpdateStateEvent += loopMethod;
        }

        public event System.Action UpdateStateEvent;

        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }

    public class RedDragonBreathEndState : IState
    {
        public RedDragonBreathEndState(System.Action endMethod)
        {
            UpdateStateEvent += endMethod;
        }

        public event System.Action UpdateStateEvent;

        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }

    public class RedDragonSpawnMinionState : IState
    {
        public RedDragonSpawnMinionState(System.Action spawnMethod)
        {
            UpdateStateEvent += spawnMethod;
        }

        public event System.Action UpdateStateEvent;

        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }

    public class RedDragonProjectileAttackState : IState
    {
        public RedDragonProjectileAttackState(System.Action attackMethod)
        {
            UpdateStateEvent += attackMethod;
        }

        public event System.Action UpdateStateEvent;

        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }

        public bool LockAnimationChange => false;
    }
    

    public class BossRedDragonController : BossController
    {
        public class BossBossRedDragonFactory : NgoZenjectFactory<BossRedDragonController>
        {
            public BossBossRedDragonFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Character/RedDragon");
            }
        }
        
        private const float BaseAttackPreFrame = 0.25f;
        
        private const float BaseAttackTransitionTime = 0.25f;
        private const float TailAttackTransitionTime = 0.3f;
        private const float JumpTransitionTime = 0.35f;
        private const float FlyMoveTransitionTime = 0.35f;
        private const float LandingTransitionTime = 0.01f;
        private const float BreathStartTransitionTime = 0.15f;
        private const float BreathLoopTransitionTime = 0.1f;
        private const float BreathEndTransitionTime = 0.1f;
        private const float SpawnMinionTransitionTime = 0.15f;
        private const float ProjectileAttackTransitionTime = 0.15f;
        
        
        private readonly Dictionary<IState, float> _stateDecelerationRatioDict = new Dictionary<IState, float>();
        public override Dictionary<IState, float> StateDecelerationRatioDict => _stateDecelerationRatioDict;

        protected override int HashIdle =>BossRedDragonAnimHash.RedDragonIdle;
        protected override int HashMove => BossRedDragonAnimHash.RedDragonMove;
        protected override int HashAttack => BossRedDragonAnimHash.RedDragonAttack;
        protected override int HashDie => BossRedDragonAnimHash.RedDragonDie;
        private int HashTailAttack => BossRedDragonAnimHash.RedDragonTailAttack;
        private int HashJump =>  BossRedDragonAnimHash.RedDragonJump;
        private int HashFlyMove => BossRedDragonAnimHash.RedDragonFlyMove;
        private int HashLanding => BossRedDragonAnimHash.RedDragonLanding;
        private int HashBreathStart => BossRedDragonAnimHash.RedDragonBreathStart;
        private int HashBreathLoop => BossRedDragonAnimHash.RedDragonBreathLoop;
        private int HashBreathEnd => BossRedDragonAnimHash.RedDragonBreathEnd;
        private int HashSpawnMinion => BossRedDragonAnimHash.RedDragonSpawnMinion;
        private int HashProjectileAttack => BossRedDragonAnimHash.RedDragonProjectileAttack;

        public override AttackState BaseAttackState => _baseAttackState;
        public override IDleState BaseIDleState => _baseIdleState;
        public override DieState BaseDieState => _baseDieState;
        public override MoveState BaseMoveState => _baseMoveState;
        public RedDragonTailAttackState TailAttackState => _tailAttackState;
        public RedDragonJumpState JumpState => _redDragonJumpState;
        public RedDragonFlyMoveState FlyMoveState => _redDragonFlyMoveState;
        public RedDragonLandingState LandingState => _redDragonLandingState;
        public RedDragonBreathStartState BreathStartState => _redDragonBreathStartState;
        public RedDragonBreathLoopState BreathLoopState => _redDragonBreathLoopState;
        public RedDragonBreathEndState BreathEndState => _redDragonBreathEndState;
        public RedDragonSpawnMinionState SpawnMinionState => _redDragonSpawnMinionState;
        public RedDragonProjectileAttackState ProjectileAttackState => _redDragonProjectileAttackState;
        public float AirborneGroundY { get; set; }
        

        private AttackState _baseAttackState;
        private IDleState _baseIdleState;
        private DieState _baseDieState;
        private MoveState _baseMoveState;
        private RedDragonTailAttackState _tailAttackState;
        private RedDragonJumpState _redDragonJumpState;
        private RedDragonFlyMoveState _redDragonFlyMoveState;
        private RedDragonLandingState _redDragonLandingState;
        private RedDragonBreathStartState _redDragonBreathStartState;
        private RedDragonBreathLoopState _redDragonBreathLoopState;
        private RedDragonBreathEndState _redDragonBreathEndState;
        private RedDragonSpawnMinionState _redDragonSpawnMinionState;
        private RedDragonProjectileAttackState _redDragonProjectileAttackState;

        protected override void AwakeInit()
        {
            base.AwakeInit();

            _baseAttackState = new AttackState(UpdateAttack);
            _baseIdleState = new IDleState(UpdateIdle);
            _baseDieState = new DieState(UpdateDie);
            _baseMoveState = new MoveState(UpdateMove);
            _tailAttackState = new RedDragonTailAttackState(UpdateTailAttack);
            _redDragonJumpState = new RedDragonJumpState(UpdateJumpState);
            _redDragonFlyMoveState = new RedDragonFlyMoveState(UpdateFlyMove);
            _redDragonLandingState = new RedDragonLandingState(UpdateLandingState);
            _redDragonBreathStartState = new RedDragonBreathStartState(UpdateBreathStart);
            _redDragonBreathLoopState = new RedDragonBreathLoopState(UpdateBreathLoop);
            _redDragonBreathEndState = new RedDragonBreathEndState(UpdateBreathEnd);
            _redDragonSpawnMinionState = new RedDragonSpawnMinionState(UpdateSpawnMinion);
            _redDragonProjectileAttackState = new RedDragonProjectileAttackState(UpdateProjectileAttack);
            TransitionAttack = BaseAttackTransitionTime;
            _stateDecelerationRatioDict.Add(_baseAttackState, BaseAttackPreFrame);
            _stateDecelerationRatioDict.Add(_tailAttackState, BaseAttackPreFrame);

        }

        protected override void StartInit()
        {
            // TODO:
            // - Cache attack point transforms.
            // - Initialize phase/state values needed by RedDragon gameplay.
        }
        public void UpdateJumpState()
        {
            if (CurrentStateType == BaseDieState)
            {
                return;
            }

            CurrentStateType = JumpState;
        }

        public void UpdateFlyMove()
        {
            if (CurrentStateType == BaseDieState)
            {
                return;
            }

            CurrentStateType = FlyMoveState;
        }

        public void UpdateLandingState()
        {
            if (CurrentStateType == BaseDieState)
            {
                return;
            }

            CurrentStateType = LandingState;
        }

        public void UpdateBreathStart()
        {
            if (CurrentStateType == BaseDieState)
            {
                return;
            }

            CurrentStateType = BreathStartState;
        }

        public void UpdateBreathLoop()
        {
            if (CurrentStateType == BaseDieState)
            {
                return;
            }

            CurrentStateType = BreathLoopState;
        }

        public void UpdateBreathEnd()
        {
            if (CurrentStateType == BaseDieState)
            {
                return;
            }

            CurrentStateType = BreathEndState;
        }

        public void UpdateSpawnMinion()
        {
            if (CurrentStateType == BaseDieState)
            {
                return;
            }

            CurrentStateType = SpawnMinionState;
        }

        public void UpdateProjectileAttack()
        {
            if (CurrentStateType == BaseDieState)
            {
                return;
            }

            CurrentStateType = ProjectileAttackState;
        }

        public override void UpdateAttack()
        {
            if (CurrentStateType == BaseDieState)
            {
                return;
            }

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
            CurrentStateType = BaseDieState;
        }

        public void UpdateTailAttack()
        {
            if (CurrentStateType == BaseDieState)
            {
                return;
            }

            CurrentStateType = _tailAttackState;
        }
        
        
        protected override void AddInitalizeStateDict()
        {
            StateAnimDict.RegisterState(_tailAttackState,
                () => RunAnimation(HashTailAttack, TailAttackTransitionTime));
            StateAnimDict.RegisterState(_redDragonJumpState,
                () => RunAnimation(HashJump, JumpTransitionTime));
            StateAnimDict.RegisterState(_redDragonFlyMoveState,
                () => RunAnimation(HashFlyMove, FlyMoveTransitionTime));
            StateAnimDict.RegisterState(_redDragonLandingState,
                () => RunAnimation(HashLanding, LandingTransitionTime));
            StateAnimDict.RegisterState(_redDragonBreathStartState,
                () => RunAnimation(HashBreathStart, BreathStartTransitionTime));
            StateAnimDict.RegisterState(_redDragonBreathLoopState,
                () => RunAnimation(HashBreathLoop, BreathLoopTransitionTime));
            StateAnimDict.RegisterState(_redDragonBreathEndState,
                () => RunAnimation(HashBreathEnd, BreathEndTransitionTime));
            StateAnimDict.RegisterState(_redDragonSpawnMinionState,
                () => RunAnimation(HashSpawnMinion, SpawnMinionTransitionTime));
            StateAnimDict.RegisterState(_redDragonProjectileAttackState,
                () => RunAnimation(HashProjectileAttack, ProjectileAttackTransitionTime));
        }
    }
}
