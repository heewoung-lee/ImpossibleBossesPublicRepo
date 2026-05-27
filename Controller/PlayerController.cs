using System;
using Controller.ControllerStats.BaseStates;
using Controller.PlayerState;
using Data;
using GameManagers.InputManagement;
using Module.PlayerModule.PlayerClassModule;
using Stats;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Util;
using Zenject;

namespace Controller
{
    public class PlayerController : MoveableController
    {
        private const float DefaultTransitionPickup = 0.3f;
        [Inject]private IInputAsset _inputmanager;
        private NavMeshAgent _agent;
        private PlayerStats _stats;
        private CrowdControl.PlayerCrowdControlNetworkReceiver _crowdControlReceiver;

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _pointerAction;
        private InputAction _attackAction;
        private InputAction _stopAction;

        private Action<Vector3> _onPlayerMouseClickPosition;

        public event Action<Vector3> OnPlayerMouseClickPosition
        {
            add { UniqueEventRegister.AddSingleEvent(ref _onPlayerMouseClickPosition, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _onPlayerMouseClickPosition, value); }
        }

        public Func<InputAction.CallbackContext, Vector3> ClickPositionEvent;
        public override Define.WorldObject WorldobjectType { get; protected set; } = Define.WorldObject.Player;
        protected override int HashIdle => PlayerAnimHash.Idle;
        protected override int HashMove => PlayerAnimHash.Run;
        protected override int HashAttack => PlayerAnimHash.Attack;
        protected override int HashDie => PlayerAnimHash.Die;
        private int HashVictory => _playerClassModule.VictoryAnimHash;
        private int _hashPickUp => Animator.StringToHash("Pickup");

        public override AttackState BaseAttackState => _baseAttackState;
        public override IDleState BaseIDleState => _baseIDleState;
        public override DieState BaseDieState => _baseDieState;
        public override MoveState BaseMoveState => _baseMoveState;
        public VictoryState BaseVictoryState => _baseVictoryState;

        public PickUpState PickupState => _pickupState;

        private AttackState _baseAttackState;
        private IDleState _baseIDleState;
        private DieState _baseDieState;
        private MoveState _baseMoveState;
        private VictoryState _baseVictoryState;
        private PickUpState _pickupState;
        private ModulePlayerClass _playerClassModule;


        protected override void AwakeInit()
        {
            _stats = gameObject.GetComponent<PlayerStats>();
            _agent = gameObject.GetComponent<NavMeshAgent>();
            _playerInput = gameObject.GetComponent<PlayerInput>();
            _crowdControlReceiver = gameObject.GetComponent<CrowdControl.PlayerCrowdControlNetworkReceiver>();
            _playerClassModule = gameObject.GetComponent<ModulePlayerClass>();

            _baseAttackState = new AttackState(UpdateAttack);
            _baseMoveState = new MoveState(UpdateMove);
            _baseDieState = new DieState(UpdateDie);
            _baseIDleState = new IDleState(UpdateIdle);
            _baseVictoryState = new VictoryState(UpdateVictory);
            _pickupState = new PickUpState(UpdatePickup);
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            _playerInput.actions = _inputmanager.GetInputActionAsset();
            _moveAction = _inputmanager.GetInputAction(Define.ControllerType.Player, "Move");
            _pointerAction = _inputmanager.GetInputAction(Define.ControllerType.Player, "Pointer");
            _attackAction = _inputmanager.GetInputAction(Define.ControllerType.Player, "Attack");
            _stopAction = _inputmanager.GetInputAction(Define.ControllerType.Player, "Stop");
            _moveAction.Enable();
            _pointerAction.Enable();
            _attackAction.Enable();
            _stopAction.Enable();
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            ClickPositionEvent += MouseRightClickPosEvent;
            _moveAction.performed += MouseRightClickEvent;
            _attackAction.performed += Attack;
            _stopAction.performed += StopCommand;
        }

        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            ClickPositionEvent -= MouseRightClickPosEvent;
            _moveAction.performed -= MouseRightClickEvent;
            _attackAction.performed -= Attack;
            _stopAction.performed -= StopCommand;
        }


        protected override void StartInit()
        {
            _stats.PlayerDeadEvent -= PlayerDead;
            _stats.PlayerDeadEvent += PlayerDead;
        }

        private Vector3 MouseRightClickPosEvent(InputAction.CallbackContext context)
        {
            Ray ray = Camera.main.ScreenPointToRay(_pointerAction.ReadValue<Vector2>());
            RaycastHit hit;

            Debug.DrawRay(Camera.main.transform.position, ray.direction * 100, Color.red);
            if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground")))
            {
                _destPos = hit.point;
                if (CurrentStateType != _baseMoveState)
                    CurrentStateType = _baseMoveState;
            }

            return hit.point;
        }

        private void MouseRightClickEvent(InputAction.CallbackContext context)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;


            Vector2 screenPos = _pointerAction.ReadValue<Vector2>();
            if (screenPos.x < 0f || screenPos.x > Screen.width ||
                screenPos.y < 0f || screenPos.y > Screen.height)
                return;

            Vector3 clickPos = MouseRightClickPosEvent(context);
            _onPlayerMouseClickPosition?.Invoke(clickPos);
        }

        public void PlayerDead()
        {
            //이쪽에 모든 애니메이션을 중단시키고 
            CurrentStateType = BaseDieState;
        }

        public void PlayVictory()
        {
            CurrentStateType = _baseVictoryState;
        }

        private void Attack(InputAction.CallbackContext context)
        {
            // Stun blocks skill input. Root still allows skill usage.
            if (_crowdControlReceiver != null && _crowdControlReceiver.IsActionLocked)
                return;

            if (CurrentStateType == BaseAttackState)
                return;

            CurrentStateType = BaseAttackState;
        }

        private void StopCommand(InputAction.CallbackContext context)
        {
            CurrentStateType = _baseIDleState;
        }

        public override void UpdateMove()
        {
            // Movement update also rotates the player, so Root/Stun both stop move and facing here.
            if (_crowdControlReceiver != null && _crowdControlReceiver.IsMovementLocked)
            {
                _agent.ResetPath();
                CurrentStateType = _baseIDleState;
                return;
            }

            Vector3 dir = new Vector3(_destPos.x, 0, _destPos.z) -
                          new Vector3(transform.position.x, 0,
                              transform.position.z); //높이에 대한 값을 빼야 근사값에 더 정확한 수치를 뽑을 수 있음.
            if (dir.magnitude < 0.01f)
            {
                CurrentStateType = _baseIDleState;
            }
            else
            {
                Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir, Color.green);
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, 1.0f, LayerMask.GetMask("Block")))
                {
                    CurrentStateType = _baseIDleState;
                    return;
                }

                float moveTick = Mathf.Clamp(_stats.MoveSpeed * Time.deltaTime, 0, dir.magnitude);
                _agent.Move(dir.normalized * moveTick);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10 * Time.deltaTime);
            }
        }

        public void UpdatePickup()
        {
            ChangeAnimIfCurrentIsDone(_hashPickUp, _baseIDleState);
        }

        public override void UpdateDie()
        {
        }

        public override void UpdateIdle()
        {
        }

        public override void UpdateAttack()
        {
            // Only Stun interrupts the attack state. Root keeps the current skill flow alive.
            if (_crowdControlReceiver != null && _crowdControlReceiver.IsActionLocked)
            {
                CurrentStateType = _baseIDleState;
                return;
            }

            ChangeAnimIfCurrentIsDone(HashAttack, _baseIDleState);
        }

        public void UpdateVictory()
        {
        }

        protected override void AddInitalizeStateDict()
        {
            StateAnimDict.RegisterState(_baseVictoryState, () => RunAnimation(HashVictory, TransitionIdle));
            StateAnimDict.RegisterState(_pickupState, () => RunAnimation(_hashPickUp, DefaultTransitionPickup));
        }

    }
}
