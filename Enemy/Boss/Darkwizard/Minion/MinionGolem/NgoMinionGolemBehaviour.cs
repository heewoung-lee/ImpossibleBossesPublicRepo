using Enemy.Boss.Darkwizard.Minion.MinionGolem;
using GameManagers.SoundManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using NetWork.Sync;
using Stats.MonsterStats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Util;
using Zenject;
using Random = UnityEngine.Random;

namespace Enemy.Boss.Darkwizard.Minion
{
    [RequireComponent(typeof(MinionGolemStats))]
    public class NgoMinionGolemBehaviour : NetworkBehaviour
    {
        private const string MinionGolemAttackPath = "Prefabs/Enemy/Minion/MinionGolemAttack";
        private const string MinionGolemAttackCueId = "MinionGolemAttackSFX";

        private static readonly int IdleAnimHash = Animator.StringToHash("Idle");
        private static readonly int MoveAnimHash = Animator.StringToHash("Move");
        private static readonly int DieAnimHash = Animator.StringToHash("Die");

        private enum MinionGolemState
        {
            Idle,
            Move,
            Dead,
        }

        private NetworkVariable<int> _stateValue =
            new((int)MinionGolemState.Idle, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<Quaternion> _moveRotationValue =
            new(Quaternion.identity, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<double> _moveStartServerTime =
            new(0d, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private IResourcesServices _resourcesServices;
        private IVFXManagerServices _vfxManagerServices;

        private MinionGolemStats _minionGolemStats;
        private SoundPlayerBinder _soundPlayerBinder;
        private Animator _animator;

        private MinionGolemState _state = MinionGolemState.Idle;
        private Quaternion _targetRotation = Quaternion.identity;
        private bool _isCatchUpInitialized;
        private float _remainingCatchUpDistance;
        private double _serverStartTime;
        private bool _isWaitingForDieAnimationEnd;

        private float _stateEndTime;
        private float _nextFireTime;

        [Inject]
        public void Construct(IResourcesServices resourcesServices, IVFXManagerServices vfxManagerServices)
        {
            _resourcesServices = resourcesServices;
            _vfxManagerServices = vfxManagerServices;
        }

        private void Awake()
        {
            _minionGolemStats = GetComponent<MinionGolemStats>();
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
            _animator = GetComponentInChildren<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            ResetLocalState();
            ApplyNetworkState();

            _minionGolemStats.IsDeadValueChagneEvent += OnIsDeadValueChanged;
            _stateValue.OnValueChanged += OnStateValueChanged;
            _moveRotationValue.OnValueChanged += OnMoveRotationChanged;
            _moveStartServerTime.OnValueChanged += OnMoveStartServerTimeChanged;

            if (IsHost == false)
            {
                return;
            }

            EnterMoveState(Random.Range(0f, 360f));
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _minionGolemStats.IsDeadValueChagneEvent -= OnIsDeadValueChanged;
            _stateValue.OnValueChanged -= OnStateValueChanged;
            _moveRotationValue.OnValueChanged -= OnMoveRotationChanged;
            _moveStartServerTime.OnValueChanged -= OnMoveStartServerTimeChanged;

            if (IsHost)
            {
                _minionGolemStats.ResetForPoolDespawn();
            }

            ResetForPoolReuse();
        }

        private void OnIsDeadValueChanged(bool previousValue, bool newValue)
        {
            if (newValue == false)
            {
                return;
            }

            EnterDeadState();
        }

        private void OnStateValueChanged(int previousValue, int newValue)
        {
            ApplyNetworkState();
        }

        private void OnMoveRotationChanged(Quaternion previousValue, Quaternion newValue)
        {
            ApplyNetworkState();
        }

        private void OnMoveStartServerTimeChanged(double previousValue, double newValue)
        {
            ApplyNetworkState();
        }

        private void ResetLocalState()
        {
            _state = MinionGolemState.Idle;
            _targetRotation = transform.rotation;
            _isCatchUpInitialized = false;
            _remainingCatchUpDistance = 0f;
            _serverStartTime = 0d;
            _isWaitingForDieAnimationEnd = false;
            _stateEndTime = 0f;
            _nextFireTime = 0f;
        }

        private void ApplyNetworkState()
        {
            _state = (MinionGolemState)_stateValue.Value;
            _targetRotation = _moveRotationValue.Value;
            _serverStartTime = _moveStartServerTime.Value;
            _isCatchUpInitialized = false;
            _remainingCatchUpDistance = 0f;

            PlayCurrentStateAnimation();
        }

        private void PlayCurrentStateAnimation()
        {
            if (_animator == null)
            {
                return;
            }

            switch (_state)
            {
                case MinionGolemState.Idle:
                    _animator.CrossFade(IdleAnimHash, 0.05f, 0);
                    break;

                case MinionGolemState.Move:
                    _animator.CrossFade(MoveAnimHash, 0.05f, 0);
                    break;

                case MinionGolemState.Dead:
                    _animator.CrossFade(DieAnimHash, 0.05f, 0);
                    break;
            }
        }

        private void EnterIdleState()
        {
            if (IsHost == false)
            {
                return;
            }

            _stateValue.Value = (int)MinionGolemState.Idle;
            _moveStartServerTime.Value = 0d;
            _stateEndTime = Time.time + _minionGolemStats.IdleDuration;
            ApplyNetworkState();
        }

        private void EnterMoveState(float yaw)
        {
            if (IsHost == false)
            {
                return;
            }

            if (TryProjectToNavMesh(transform.position, out Vector3 navMeshPosition))
            {
                transform.position = navMeshPosition;
            }

            _stateValue.Value = (int)MinionGolemState.Move;
            _moveRotationValue.Value = Quaternion.Euler(0f, yaw, 0f);
            _moveStartServerTime.Value = NetworkManager.ServerTime.Time;
            _stateEndTime = Time.time + Random.Range(
                _minionGolemStats.MinPatrolDuration,
                _minionGolemStats.MaxPatrolDuration);
            _nextFireTime = Time.time + _minionGolemStats.FireInterval;
            ApplyNetworkState();
        }

        private void EnterDeadState()
        {
            _state = MinionGolemState.Dead;
            _isCatchUpInitialized = false;
            _remainingCatchUpDistance = 0f;
            _serverStartTime = 0d;
            _stateEndTime = 0f;
            _nextFireTime = 0f;
            _isWaitingForDieAnimationEnd = true;

            if (IsHost)
            {
                _stateValue.Value = (int)MinionGolemState.Dead;
                _moveStartServerTime.Value = 0d;
            }

            PlayCurrentStateAnimation();
        }

        public void ResetForPoolReuse()
        {
            ResetLocalState();

            if (_animator != null)
            {
                _animator.Play(IdleAnimHash, 0, 0f);
                _animator.Update(0f);
            }
        }

        private void FixedUpdate()
        {
            switch (_state)
            {
                case MinionGolemState.Idle:
                    UpdateIdle();
                    break;

                case MinionGolemState.Move:
                    UpdateMove();
                    break;

                case MinionGolemState.Dead:
                    UpdateDead();
                    break;
            }
        }

        private void UpdateIdle()
        {
            if (IsHost == false)
            {
                return;
            }

            if (Time.time < _stateEndTime)
            {
                return;
            }

            if (TryFindNextPatrolYaw(out float nextYaw))
            {
                EnterMoveState(nextYaw);
                return;
            }

            EnterIdleState();
        }

        private void UpdateMove()
        {
            float moveSpeed = _minionGolemStats.MoveSpeed;
            if (moveSpeed <= 0f)
            {
                return;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                _targetRotation,
                _minionGolemStats.TurnSpeed * Time.fixedDeltaTime);

            float baseDistance = moveSpeed * Time.fixedDeltaTime;
            float extraDistance = ChaseCatchUpCalculator.ConsumeExtraDistance(
                NetworkManager,
                _serverStartTime,
                ref _isCatchUpInitialized,
                ref _remainingCatchUpDistance,
                Time.fixedDeltaTime,
                moveSpeed,
                _minionGolemStats.CatchUpDuration,
                _minionGolemStats.MaxCatchUpMultiplier);

            float finalDistance = baseDistance + extraDistance;
            MoveOnNavMesh(finalDistance);

            if (IsHost == false)
            {
                return;
            }

            if (Time.time >= _nextFireTime)
            {
                FireProjectile();
                _nextFireTime = Time.time + _minionGolemStats.FireInterval;
            }

            if (Time.time < _stateEndTime)
            {
                return;
            }

            EnterIdleState();
        }

        private void FireProjectile()
        {
            PlayAttackSfxClientRpc();

            int projectileCount = _minionGolemStats.ProjectileCountPerShot;
            float angleStep = 360f / projectileCount;

            for (int i = 0; i < projectileCount; i++)
            {
                Quaternion shotRotation = Quaternion.Euler(0f, angleStep * i, 0f);
                _vfxManagerServices.InstantiateParticleWithTarget(
                    MinionGolemAttackPath,
                    transform,
                    shotRotation,
                    4f);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PlayAttackSfxClientRpc()
        {
            _soundPlayerBinder.PlayDetached(MinionGolemAttackCueId);
        }

        private void MoveOnNavMesh(float distance)
        {
            if (distance <= 0f)
            {
                return;
            }

            Vector3 currentPosition = transform.position;
            Vector3 desiredPosition = currentPosition + transform.forward * distance;

            if (TryProjectToNavMesh(currentPosition, out Vector3 currentNavMeshPosition))
            {
                currentPosition = currentNavMeshPosition;
            }

            if (NavMesh.Raycast(currentPosition, desiredPosition, out NavMeshHit navMeshHit, NavMesh.AllAreas))
            {
                transform.position = navMeshHit.position;

                if (IsHost)
                {
                    EnterIdleState();
                }

                return;
            }

            if (TryProjectToNavMesh(desiredPosition, out Vector3 nextNavMeshPosition))
            {
                transform.position = nextNavMeshPosition;
                return;
            }

            transform.position = desiredPosition;
        }

        private bool TryFindNextPatrolYaw(out float yaw)
        {
            yaw = transform.eulerAngles.y;

            if (TryProjectToNavMesh(transform.position, out Vector3 origin) == false)
            {
                return false;
            }

            for (int i = 0; i < 12; i++)
            {
                float candidateYaw = Random.Range(0f, 360f);
                Vector3 direction = Quaternion.Euler(0f, candidateYaw, 0f) * Vector3.forward;
                Vector3 candidate = origin + direction * _minionGolemStats.PatrolDistance;

                if (TryProjectToNavMesh(candidate, out Vector3 target) == false)
                {
                    continue;
                }

                if (NavMesh.Raycast(origin, target, out _, NavMesh.AllAreas))
                {
                    continue;
                }

                yaw = candidateYaw;
                return true;
            }

            return false;
        }

        private bool TryProjectToNavMesh(Vector3 position, out Vector3 navMeshPosition)
        {
            if (NavMesh.SamplePosition(
                    position,
                    out NavMeshHit navMeshHit,
                    _minionGolemStats.NavMeshSampleDistance,
                    NavMesh.AllAreas))
            {
                navMeshPosition = navMeshHit.position;
                return true;
            }

            navMeshPosition = position;
            return false;
        }

        private void UpdateDead()
        {
            if (_isWaitingForDieAnimationEnd == false)
            {
                return;
            }

            if (_animator == null)
            {
                _isWaitingForDieAnimationEnd = false;
                if (IsHost)
                {
                    _resourcesServices.DestroyObject(gameObject);
                }
                return;
            }

            if (_animator.IsInTransition(0))
            {
                return;
            }

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != DieAnimHash)
            {
                return;
            }

            if (stateInfo.normalizedTime < 1f)
            {
                return;
            }

            _isWaitingForDieAnimationEnd = false;

            if (IsHost)
            {
                _resourcesServices.DestroyObject(gameObject);
            }
        }
    }
}
