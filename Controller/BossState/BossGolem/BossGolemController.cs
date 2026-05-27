using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Controller.ControllerStats;
using Controller.ControllerStats.BaseStates;
using Data;
using UnityEngine;

namespace Controller.BossState.BossGolem
{
    public enum GolemAttackType
    {
        NormalAttack,
        Skill1,
        Skill2,
    }
    public class BossGolemController : BossController
    {

        private const float AttackPreFrame = 0.35f;
        private const float Skill1PreFrame = 0.64f;
        private const float Skill2PreFrame = 0.3f;
        private const float Skill1Transition = 0.1f;
        
        private Dictionary<IState, float> _stateDecelerationRatioDict = new Dictionary<IState, float>();
        public override Dictionary<IState, float> StateDecelerationRatioDict => _stateDecelerationRatioDict;

        private int[] _golemAttacks = new int[2]
        {
            Animator.StringToHash("Golem_Attack1"),
            Animator.StringToHash("Golem_Attack2")
        };

        protected override int HashIdle => BossGolemAnimHash.GolemIdle;
        protected override int HashMove => BossGolemAnimHash.GolemWalk;
        protected override int HashAttack => _golemAttacks[UnityEngine.Random.Range(0, 2)];
        protected override int HashDie => BossGolemAnimHash.GolemDead;

        private int _hashGolemSkill1 = BossGolemAnimHash.GolemAttacked;
        private int _hashGolemSkill2 = BossGolemAnimHash.GolemSkill;
        private int _hashGolemSpawnRock = BossGolemAnimHash.GolemSpawnRock;

        public override AttackState BaseAttackState => _baseAttackState;
        public override IDleState BaseIDleState => _baseIDleState;
        public override DieState BaseDieState => _baseDieState;
        public override MoveState BaseMoveState => _baseMoveState;
        public BossSkill1State BossSkill1State => _bossSkill1State;
        public BossSkill2State BossSkill2State => _bossSkill2State;
        public BossSpawnRockState BossSpawnRockState => _bossSpawnRockState;


        private AttackState _baseAttackState;
        private IDleState _baseIDleState;
        private DieState _baseDieState;
        private MoveState _baseMoveState;
        private BossSkill1State _bossSkill1State;
        private BossSkill2State _bossSkill2State;
        private BossSpawnRockState _bossSpawnRockState;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            
            _baseAttackState = new AttackState(UpdateAttack);
            _baseMoveState = new MoveState(UpdateMove);
            _baseDieState = new DieState(UpdateDie);
            _baseIDleState = new IDleState(UpdateIdle);

            _bossSkill1State = new BossSkill1State(UpdateAttack);
            _bossSkill2State = new BossSkill2State(UpdateAttack);
            _bossSpawnRockState = new BossSpawnRockState(UpdateAttack);
            TransitionAttack = AttackPreFrame;

            _stateDecelerationRatioDict.Add(_baseAttackState, AttackPreFrame);
            _stateDecelerationRatioDict.Add(_bossSkill1State, Skill1PreFrame);
            _stateDecelerationRatioDict.Add(_bossSkill2State, Skill2PreFrame);
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
        private void Update()
        {
        }
        public override void UpdateDie()
        {
            if (CurrentStateType == BaseDieState)
            {
                CurrentStateType = BaseDieState;
            }
        }

        protected override void AddInitalizeStateDict()
        {
            StateAnimDict.RegisterState(_bossSkill1State, () => RunAnimation(_hashGolemSkill1, Skill1Transition));
            StateAnimDict.RegisterState(_bossSkill2State, () => RunAnimation(_hashGolemSkill2, TransitionAttack));
            StateAnimDict.RegisterState(_bossSpawnRockState, () => RunAnimation(_hashGolemSpawnRock, TransitionAttack));
        }

        protected override void StartInit()
        {
        }
    }
}
