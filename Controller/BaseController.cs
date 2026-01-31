using System.Runtime.CompilerServices;
using Controller.ControllerStats;
using Controller.ControllerStats.BaseStates;
using CoreScripts;
using UnityEngine;
using Util;

namespace Controller
{
    public abstract class BaseController : ZenjectMonoBehaviour
    {
        public abstract Define.WorldObject WorldobjectType { get; protected set; }

        public bool IsAnimationLocked => _currentStateType != null && _currentStateType.LockAnimationChange;

        private IState _currentStateType;
        private const float DefalutTransitionIdle = 0.3f;
        private const float DefalutTransitionMove = 0.15f;
        private const float DefalutTransitionAttack = 0.01f;
        private const float DefalutTransitionDie = 0.3f;

        private Animator _anim;
        private float _transitionIdle = DefalutTransitionIdle;
        private float _transitionMove = DefalutTransitionMove;
        private float _transitionAttack = DefalutTransitionAttack;
        private float _transitionDie = DefalutTransitionDie;
        private int _animLayer;

        private GameObject _targetObject;

        public virtual GameObject TargetObject
        {
            get => _targetObject;
            set => _targetObject = value;
        }


        private StateAnimationDict _stateAnimDict = new StateAnimationDict(); //스테이터스가 바뀌면 애니메이션을 호출하는 딕셔너리
        public StateAnimationDict StateAnimDict => _stateAnimDict;

        protected abstract int HashIdle { get; }
        protected abstract int HashMove { get; }
        protected abstract int HashAttack { get; }
        protected abstract int HashDie { get; }

        public abstract AttackState BaseAttackState { get; }
        public abstract IDleState BaseIDleState { get; }
        public abstract DieState BaseDieState { get; }
        public abstract MoveState BaseMoveState { get; }

        public abstract void UpdateAttack();
        public abstract void UpdateIdle();
        public abstract void UpdateMove();
        public abstract void UpdateDie();


        public Animator Anim
        {
            get => _anim;
            protected set => _anim = value;
        }

        public float TransitionIdle
        {
            get => _transitionIdle;
            protected set => _transitionIdle = value;
        }

        public float TransitionMove
        {
            get => _transitionMove;
            protected set => _transitionMove = value;
        }

        public float TransitionAttack
        {
            get => _transitionAttack;
            protected set => _transitionAttack = value;
        }

        public float TransitionDie
        {
            get => _transitionDie;
            protected set => _transitionDie = value;
        }

        public int AnimLayer
        {
            get => _animLayer;
            protected set => _animLayer = value;
        }


        public IState CurrentStateType
        {
            get
            {
                return _currentStateType;
            }
            set
            {
                //09.09 수정 lockingAntiomation 실행도중 플레이어가 사망하면 사망 모션이 와서 조건을 추가해줌
                // 1.8 수정: 락 중이라도 '같은 상태 인스턴스'로 재진입하는 경우는 허용한다.
                // 이유:
                // 스킬 실행 시 CommonSkillState.Prepare()가 먼저 CurrentAnimHash / Lock 여부를 갱신한다.
                // 그런데 거의 동시에 다른 스킬이 들어오면 Prepare()로 CurrentAnimHash가 덮어써진 뒤,
                // CurrentStateType 변경이 락에 막혀 return 될 수 있다.
                // 그러면 실제 Animator는 이전 애니(A)를 재생 중인데, 검사 기준(CurrentAnimHash)은 B로 바뀌어,
                // CommonSkillState.UpdateState()가 B 애니가 끝나길 기다리게 된다.
                // B 애니는 실제로 재생되지 않았으므로 완료 조건이 만족되지 않아 락이 풀리지 않는(멈춤) 현상이 발생한다.
                // 따라서 같은 상태(CommonSkillState)로의 재진입은 허용해 CallState/RunAnimation이 다시 실행되도록 한다.
                if (_currentStateType.LockAnimationChange && value != BaseDieState && value != _currentStateType)
                    return;

                _currentStateType = value;
                _stateAnimDict.CallState(_currentStateType); // 현재 상태의 루프문 실행
            }
        }

        //1.29일 부활 때문에 만들었다.
        public void ForceChangeState(IState newState)
        {
            _currentStateType = newState;
            _stateAnimDict.CallState(_currentStateType); 
        }


        public void ChangeAnimIfCurrentIsDone(int currentAnimHash, IState changeState)
        {
            if (IsAnimationDone(currentAnimHash) == false)
                return;


            if (CurrentStateType.LockAnimationChange)
            {
                _currentStateType = changeState;
                _stateAnimDict.CallState(_currentStateType);
            }
            else
            {
                CurrentStateType = changeState;
            }
        }

        public bool IsAnimationDone(int animHash)
        {
            AnimatorStateInfo stateInfo = Anim.GetCurrentAnimatorStateInfo(AnimLayer);
            //  스테이트가 정상 재생 중이며, 재생이 끝났는지 검사
            if (Anim.IsInTransition(AnimLayer) == false 
                && stateInfo.shortNameHash == animHash 
                && stateInfo.normalizedTime >= 1.0f 
                && stateInfo.loop == false) // 1.24일 루프 추가 채널링 스킬을 만들때 애니메이션이 루프로 돌아가야 해서 루프 조건을 추가함
                return true;

            return false;
        }

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            AwakeInit();
            InitailizeStateDict(); //기본 스테이터스 초기화
            _currentStateType = BaseIDleState; //기본 스테이터스 지정
        }

        private void Start()
        {
            StartInit();
        }

        protected abstract void AwakeInit();
        protected abstract void StartInit();

        public void SetDefalutTransition_Value()
        {
            _transitionIdle = DefalutTransitionIdle;
            _transitionMove = DefalutTransitionMove;
            _transitionAttack = DefalutTransitionAttack;
            _transitionDie = DefalutTransitionDie;
        }

        protected abstract void AddInitalizeStateDict();

        private void InitailizeStateDict()
        {
            _stateAnimDict.RegisterState(BaseAttackState, () => RunAnimation(HashAttack, TransitionAttack));
            _stateAnimDict.RegisterState(BaseDieState, () => RunAnimation(HashDie, TransitionDie));
            _stateAnimDict.RegisterState(BaseIDleState, () => RunAnimation(HashIdle, TransitionIdle));
            _stateAnimDict.RegisterState(BaseMoveState, () => RunAnimation(HashMove, TransitionMove));
            AddInitalizeStateDict();
        }


        public void RunAnimation(int hashCode, float transitionState)
        {
            if (_anim.HasState(0, hashCode))
            {
                _anim.CrossFade(hashCode, transitionState, AnimLayer, 0f);
            }
            else
            {
                Debug.LogError($"[BaseController] 애니메이터에 존재하지 않는 State Hash입니다! Hash: {hashCode}");
            }
        }
    }
}