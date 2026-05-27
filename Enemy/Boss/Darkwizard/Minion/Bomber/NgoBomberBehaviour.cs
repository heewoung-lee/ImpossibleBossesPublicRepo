using Enemy.Boss.Darkwizard.Minion.Bomber;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using GameManagers.VFXManagement;
using NetWork.NGO;
using NetWork.Sync;
using Stats;
using Stats.MonsterStats;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace Enemy.Boss.Darkwizard.Minion
{
    [RequireComponent(typeof(BomberStats))]
    [RequireComponent(typeof(BomberFlashEffect))]
    [RequireComponent(typeof(SoundPlayerBinder))]
    public class NgoBomberBehaviour : NetworkBehaviour
    {
        private const string BomberGaugeCueId = "MinionBomberGaugeSFX";
        private static readonly int DieAnimHash = Animator.StringToHash("Die");
        private static readonly int IdleAnimHash = Animator.StringToHash("Idle");

        private enum BomberState
        {
            Idle,
            Move,
            Dead,
        }

        private NetworkVariable<NetworkObjectReference> _targetRef =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<double> _chaseStartServerTime =
            new(0d, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> _stateValue =
            new((int)BomberState.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> _isFlashStarted =
            new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private RelayManager _relayManager;
        private BomberStats _bomberStats;
        private BomberFlashEffect _flashEffect;
        private SoundPlayerBinder _soundPlayerBinder;
        private Animator _animator;
        private IVFXManagerServices _vfxManagerServices;
        private IResourcesServices _resourcesServices;
        
        
        [Inject]
        public void Construct(RelayManager relayManager,IVFXManagerServices vfxManagerServices,IResourcesServices resourcesServices)
        {
            _relayManager = relayManager;
            _vfxManagerServices = vfxManagerServices;
            _resourcesServices = resourcesServices;
        }


        private void Awake()
        {
            _bomberStats = GetComponent<BomberStats>();
            _flashEffect = GetComponent<BomberFlashEffect>();
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
            _animator = GetComponentInChildren<Animator>();
        }


      
        private BomberState _state = BomberState.Idle;
        private Transform _target;

        private bool _isCatchUpInitialized;
        private float _remainingCatchUpDistance;
        private double _serverStartTime;
        private bool _isWaitingForDieAnimationEnd;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            ResetLocalState();
            ApplyNetworkState();


            _flashEffect.FlashDoneEvent += Explosion;
            _bomberStats.IsDeadValueChagneEvent += OnIsDeadValueChanged;
            _targetRef.OnValueChanged += OnTargetRefChanged;
            _chaseStartServerTime.OnValueChanged += OnChaseStartTimeChanged;
            _stateValue.OnValueChanged += OnStateValueChanged;
            _isFlashStarted.OnValueChanged += OnFlashStartedChanged;

            if (IsHost == false)
            {
                return;
            }

           

            GameObject targetObject = EnemyFindTarget.FindRandomPlayer(_relayManager.NetworkManagerEx);
            if (targetObject == null)
            {
                UtilDebug.Log("타겟이 없습니다.");
                return;
            }

            if (targetObject.TryGetComponent(out NetworkObject targetNetworkObject) == false)
            {
                UtilDebug.Log("타겟이 네트워크 오브젝트가 아닙니다.");
                return;
            }

            _targetRef.Value = new NetworkObjectReference(targetNetworkObject);
            _chaseStartServerTime.Value = NetworkManager.ServerTime.Time;
            _stateValue.Value = (int)BomberState.Move;

            ApplyNetworkState();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _flashEffect.FlashDoneEvent -= Explosion;
            _bomberStats.IsDeadValueChagneEvent -= OnIsDeadValueChanged;
            _targetRef.OnValueChanged -= OnTargetRefChanged;
            _chaseStartServerTime.OnValueChanged -= OnChaseStartTimeChanged;
            _stateValue.OnValueChanged -= OnStateValueChanged;
            _isFlashStarted.OnValueChanged -= OnFlashStartedChanged;

            if (IsHost)
            {
                _bomberStats.ResetForPoolDespawn();
            }

            ResetForPoolReuse();
        }

        private void OnTargetRefChanged(NetworkObjectReference previousValue, NetworkObjectReference newValue)
        {
            ApplyNetworkState();
        }

        private void OnChaseStartTimeChanged(double previousValue, double newValue)
        {
            ApplyNetworkState();
        }

        private void OnStateValueChanged(int previousValue, int newValue)
        {
            ApplyNetworkState();
        }

        private void OnFlashStartedChanged(bool previousValue, bool newValue)
        {
            if (newValue == false)
            {
                return;
            }

            _flashEffect.PlayFlash();
            PlayFlashStartSfx();
        }

        private void OnIsDeadValueChanged(bool previousValue, bool newValue)
        {
            if (newValue == false)
            {
                return;
            }

            EnterDeadState();
        }

        private void ResetLocalState()
        {
            _state = BomberState.Idle;
            _target = null;
            _isCatchUpInitialized = false;
            _remainingCatchUpDistance = 0f;
            _serverStartTime = 0d;
            _isWaitingForDieAnimationEnd = false;
            _flashEffect.ResetFlash();
        }

        private void ApplyNetworkState()
        {
            _state = (BomberState)_stateValue.Value;

            if (_targetRef.Value.TryGet(out NetworkObject targetNetworkObject))
            {
                _target = targetNetworkObject.transform;
            }
            else
            {
                _target = null;
            }

            _serverStartTime = _chaseStartServerTime.Value;
            _isCatchUpInitialized = false;
            _remainingCatchUpDistance = 0f;
        }

        private void EnterDeadState()
        {
            _state = BomberState.Dead;
            _target = null;
            _isCatchUpInitialized = false;
            _remainingCatchUpDistance = 0f;
            _serverStartTime = 0d;
            _isWaitingForDieAnimationEnd = true;
            _flashEffect.ResetFlash();

            if (IsHost)
            {
                _targetRef.Value = default;
                _chaseStartServerTime.Value = 0d;
                _isFlashStarted.Value = false;

                if (_stateValue.Value != (int)BomberState.Dead)
                {
                    _stateValue.Value = (int)BomberState.Dead;
                }
            }

            PlayDieAnimation();
        }

        private void PlayDieAnimation()
        {
            if (_animator == null)
            {
                return;
            }

            _animator.CrossFade(DieAnimHash, 0.05f, 0);
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
                case BomberState.Idle:
                    UpdateIdle();
                    break;

                case BomberState.Move:
                    UpdateMove();
                    break;

                case BomberState.Dead:
                    UpdateDead();
                    break;
            }
        }


        private void Explosion()
        {
            if (IsHost == false) return;//호스트만 판정을 갖게함
            
            //데미지 넣고
            //넉백넣고
            //효과 넣고
            //삭제
            if (_bomberStats.IsDead)
                return;

            ApplyExplosionEffects();
            _isFlashStarted.Value = false;
            _vfxManagerServices.InstantiateParticleInArea("Prefabs/Enemy/Minion/BomberExplosion",transform.position);
            _resourcesServices.DestroyObject(gameObject);    
            
        }
        
        
        private void ApplyExplosionEffects()
        {
            Collider[] hitColliders = Physics.OverlapSphere(
                transform.position,
                _bomberStats.ExplosionRadius,
                _bomberStats.TarGetLayer);

            HashSet<PlayerStats> hitPlayers = new HashSet<PlayerStats>();
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.transform.TryGetComponentInParents(out PlayerStats playerStats) == false)
                    continue;

                if (hitPlayers.Add(playerStats) == false)
                    continue;

                ApplyExplosionDamage(playerStats);
                ApplyExplosionPush(playerStats);
            }
        }

        private void ApplyExplosionDamage(PlayerStats playerStats)
        {
            playerStats.OnAttacked(_bomberStats, _bomberStats.Attack);
        }

        private void ApplyExplosionPush(PlayerStats playerStats)
        {
            if (playerStats.transform.TryGetComponentInParents(out PlayerInitializeNgo playerInitializeNgo) == false)
                return;

            Vector3 pushDir = playerStats.transform.position - transform.position;
            pushDir.y = 0f;

            if (pushDir.sqrMagnitude <= 0.0001f)
            {
                pushDir = transform.forward;
            }
            else
            {
                pushDir.Normalize();
            }

            playerInitializeNgo.PushBackFromNetworkRpc(
                pushDir,
                _bomberStats.PushDistance,
                _bomberStats.PushDuration);
        }

        private void UpdateIdle()
        {
        }

        private void PlayFlashStartSfx()
        {
            if (_soundPlayerBinder.TryGetClip(BomberGaugeCueId, out AudioClip clip) == false)
            {
                return;
            }

            float pitch = clip.length / Mathf.Max(_bomberStats.FlashDuration, 0.01f);
            _soundPlayerBinder.PlayDetached(BomberGaugeCueId, pitch);
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

        private void UpdateMove()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 directionToTarget = _target.position - transform.position;
            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float distanceToTarget = directionToTarget.magnitude;
            Vector3 normalizedDirection = directionToTarget / distanceToTarget;
            Quaternion targetRotation = Quaternion.LookRotation(normalizedDirection);
            float moveSpeed = _bomberStats.MoveSpeed;

            if (moveSpeed <= 0f)
            {
                return;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _bomberStats.TurnSpeed * Time.fixedDeltaTime);

            float baseDistance = moveSpeed * Time.fixedDeltaTime;

            float extraDistance = ChaseCatchUpCalculator.ConsumeExtraDistance(
                NetworkManager,
                _serverStartTime,
                ref _isCatchUpInitialized,
                ref _remainingCatchUpDistance,
                Time.fixedDeltaTime,
                moveSpeed,
                _bomberStats.CatchUpDuration,_bomberStats.MaxCatchUpMultiplier);

            float finalDistance = baseDistance + extraDistance;
            transform.position += transform.forward * finalDistance;

            if (IsHost == false)
            {
                return;
            }

            if (distanceToTarget <= _bomberStats.BombRange)
            {
                UtilDebug.Log($"[NgoBomberBehaviour] Bomb range reached. Target: {_target?.name}");
                _stateValue.Value = (int)BomberState.Idle;
                _targetRef.Value = default;
                _chaseStartServerTime.Value = 0d;
                _isFlashStarted.Value = true;
                ApplyNetworkState();
            }
        }
    }
}
