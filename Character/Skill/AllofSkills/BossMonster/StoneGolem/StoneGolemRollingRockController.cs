using Unity.Netcode;
using UnityEngine;
using Stats.MonsterStats;
using Stats;
using Util;
using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using NetWork.NGO;
using Controller.CrowdControl;
using GameManagers.SoundManagement;
using Zenject;

namespace BehaviourTreeNode.BossGolem.Task
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(MinionRockStats))]
    [RequireComponent(typeof(SoundPlayerBinder))]
    public class StoneGolemRollingRockController : NetworkBehaviour
    {
        private const string MinionRockVfxPath = "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoMinionRockVFX";
        private const string StunVfxPath = "Prefabs/Player/VFX/Common/NgoStunVFX";
        private const string DribbleCueId = "MinionRockSFX";
        private const string BrokenCueId = "MinionRockBrokenSFX";
        private const float ClientPositionLerpSpeed = 20f;
        private const float ClientRotationDegreesPerSecondAtUnitSpeed = 90f;
        private const float GroundRayExtraHeight = 1f;
        private const float GroundRayDistance = 10f;
        private const float GroundSnapPadding = 0.02f;
        private const float ShatterReturnDelay = 3f;
        private const float ShatterImpulseForce = 6f;
        private const float ShatterUpwardBias = 0.35f;
        private const float PlayerStunDuration = 5f;
        private const float PlayerPushDistance = 2f;
        private const float PlayerPushDuration = 0.25f;

        private readonly NetworkVariable<Vector3> _networkPosition = new NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<Vector3> _moveDirection = new NetworkVariable<Vector3>(
            Vector3.forward,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> _isRolling = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private Rigidbody _rigidbody;
        private CapsuleCollider _capsuleCollider;
        private MinionRockStats _minionRockStats;
        private SoundPlayerBinder _soundPlayerBinder;
        private StoneGolemRollingRockShardShrink _shardShrink;
        private Transform[] _visualChildren;
        private Rigidbody[] _childRigidbodies;
        private Collider[] _childColliders;
        private Renderer[] _renderers;
        private int _groundLayerMask;
        private int _wallLayerMask;
        private int _playerLayerMask;
        private IResourcesServices _resourcesServices;
        private IVFXManagerServices _vfxManagerServices;
        private bool _hasShattered;
        private bool _hasReceivedInitialPosition;

        [Inject]
        public void Construct(IResourcesServices resourcesServices, IVFXManagerServices vfxManagerServices)
        {
            _resourcesServices = resourcesServices;
            _vfxManagerServices = vfxManagerServices;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _minionRockStats = GetComponent<MinionRockStats>();
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
            _shardShrink = GetComponent<StoneGolemRollingRockShardShrink>();
            _visualChildren = GetComponentsInChildren<Transform>(true);
            _childRigidbodies = GetComponentsInChildren<Rigidbody>(true);
            _childColliders = GetComponentsInChildren<Collider>(true);
            _renderers = GetComponentsInChildren<Renderer>(true);
            _groundLayerMask = LayerMask.GetMask("Ground");
            _wallLayerMask = LayerMask.GetMask("Wall");
            _playerLayerMask = LayerMask.GetMask(
                Utill.GetLayerID(Define.ControllerLayer.Player),
                Utill.GetLayerID(Define.ControllerLayer.AnotherPlayer));

            ConfigureForRolling();
            DisableChildPhysics();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _hasReceivedInitialPosition = IsServer;
            PrepareForRolling();
            // 2026-05-22: 클라이언트에서 NgoMinionRock이 첫 위치 동기화 전에
            // 풀의 이전 위치에 1프레임 보인 문제가 있었다.
            // 순수 클라이언트는 첫 네트워크 위치를 적용할 때까지 렌더러를 숨긴다.
            SetRenderersVisible(IsServer);
            _minionRockStats.IsDeadValueChagneEvent += OnIsDeadValueChanged;
            _networkPosition.OnValueChanged += OnNetworkPositionChanged;
            _isRolling.OnValueChanged += OnRollingValueChanged;
            UpdateDribbleLoop(_isRolling.Value);

            if (IsServer == false && _isRolling.Value)
            {
                ApplyFirstClientNetworkPosition(_networkPosition.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _minionRockStats.IsDeadValueChagneEvent -= OnIsDeadValueChanged;
            _networkPosition.OnValueChanged -= OnNetworkPositionChanged;
            _isRolling.OnValueChanged -= OnRollingValueChanged;
            _hasReceivedInitialPosition = false;
            // 풀 재사용 때 숨김 상태가 남지 않도록 반환 시 렌더러 상태를 복구한다.
            SetRenderersVisible(true);
            UpdateDribbleLoop(false);
            ResetRollingState();

            if (IsHost && _minionRockStats != null)
            {
                _minionRockStats.ResetForPoolDespawn();
            }
        }

        public void InitializeRolling(Vector3 direction)
        {
            if (IsServer == false)
            {
                return;
            }

            if (_minionRockStats == null)
            {
                UtilDebug.LogError($"[{nameof(StoneGolemRollingRockController)}] {nameof(MinionRockStats)} is missing.");
                return;
            }

            PrepareForRolling();

            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = Vector3.forward;
            }

            _moveDirection.Value = direction.normalized;

            Vector3 alignedPosition = ResolveGroundPosition(transform.position);
            _rigidbody.position = alignedPosition;
            transform.position = alignedPosition;
            _networkPosition.Value = alignedPosition;
            _isRolling.Value = true;
        }

        private void OnIsDeadValueChanged(bool previousValue, bool newValue)
        {
            if (newValue == false || _hasShattered)
            {
                return;
            }

            CollapseShatter();
        }

        private void OnRollingValueChanged(bool previousValue, bool newValue)
        {
            UpdateDribbleLoop(newValue);

            if (IsServer == false && newValue && _hasReceivedInitialPosition == false)
            {
                ApplyFirstClientNetworkPosition(_networkPosition.Value);
            }
        }

        private void OnNetworkPositionChanged(Vector3 previousValue, Vector3 newValue)
        {
            if (IsServer || _hasShattered || _hasReceivedInitialPosition)
            {
                return;
            }

            ApplyFirstClientNetworkPosition(newValue);
        }

        private void ApplyFirstClientNetworkPosition(Vector3 position)
        {
            transform.position = position;
            _rigidbody.position = position;
            _hasReceivedInitialPosition = true;
            SetRenderersVisible(true);
        }

        private void SetRenderersVisible(bool isVisible)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = isVisible;
            }
        }

        private void FixedUpdate()
        {
            if (IsSpawned == false || _hasShattered)
            {
                return;
            }

            if (IsServer)
            {
                if (_isRolling.Value == false)
                {
                    return;
                }

                float currentMoveSpeed = GetCurrentMoveSpeed();
                if (currentMoveSpeed <= 0f)
                {
                    return;
                }

                Vector3 nextPosition = _rigidbody.position + _moveDirection.Value * (currentMoveSpeed * Time.fixedDeltaTime);
                nextPosition = ResolveGroundPosition(nextPosition);
                _rigidbody.MovePosition(nextPosition);
                _networkPosition.Value = nextPosition;
                return;
            }

            if (_isRolling.Value == false || _hasReceivedInitialPosition == false)
            {
                return;
            }

            transform.position = Vector3.Lerp(
                transform.position,
                _networkPosition.Value,
                ClientPositionLerpSpeed * Time.fixedDeltaTime);
        }

        private void Update()
        {
            if (IsSpawned == false || _isRolling.Value == false || _hasShattered)
            {
                return;
            }

            RotateVisualChildren(_moveDirection.Value, GetCurrentMoveSpeed(), Time.deltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsServer == false || _isRolling.Value == false || _hasShattered)
            {
                return;
            }

            if (IsPlayerLayer(collision.gameObject.layer))
            {
                Vector3 hitPoint = collision.contactCount > 0
                    ? collision.contacts[0].point
                    : transform.position;
                ApplyPlayerHit(collision);
                ExplodeShatter(hitPoint);
                return;
            }

            if (IsWallLayer(collision.gameObject.layer) == false || collision.contactCount <= 0)
            {
                return;
            }

            Vector3 reflectedDirection = Vector3.Reflect(_moveDirection.Value, collision.contacts[0].normal);
            reflectedDirection.y = 0f;

            if (reflectedDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            _moveDirection.Value = reflectedDirection.normalized;
        }

        private void ConfigureForRolling()
        {
            _capsuleCollider.isTrigger = false;
            _capsuleCollider.enabled = true;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = false;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void DisableChildPhysics()
        {
            for (int i = 0; i < _childRigidbodies.Length; i++)
            {
                Rigidbody childRigidbody = _childRigidbodies[i];
                if (childRigidbody == _rigidbody)
                {
                    continue;
                }

                childRigidbody.useGravity = false;
                childRigidbody.detectCollisions = false;
                childRigidbody.isKinematic = true;
            }

            for (int i = 0; i < _childColliders.Length; i++)
            {
                Collider childCollider = _childColliders[i];
                if (childCollider == _capsuleCollider)
                {
                    continue;
                }

                childCollider.enabled = false;
            }
        }

        private void PrepareForRolling()
        {
            _hasShattered = false;
            ConfigureForRolling();
            DisableChildPhysics();
        }

        private float GetCurrentMoveSpeed()
        {
            if (_minionRockStats == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, _minionRockStats.MoveSpeed);
        }

        private void ResetRollingState()
        {
            UpdateDribbleLoop(false);

            if (IsServer)
            {
                _isRolling.Value = false;
                _moveDirection.Value = Vector3.zero;
                _networkPosition.Value = transform.position;
            }

            PrepareForRolling();
        }

        private void CollapseShatter()
        {
            EnterShatterState(Vector3.zero);
        }

        private void ExplodeShatter(Vector3 vfxSpawnPosition)
        {
            EnterShatterState(GetLocalShatterRootVelocity());
            ExplodeShatterClientRpc();

            if (IsHost)
            {
                SpawnShatterVfx(vfxSpawnPosition);
            }
        }

        private Vector3 GetLocalShatterRootVelocity()
        {
            Vector3 moveDirection = _moveDirection.Value;
            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return Vector3.zero;
            }

            return moveDirection.normalized * GetCurrentMoveSpeed();
        }

        private void EnterShatterState(Vector3 rootVelocity)
        {
            _hasShattered = true;
            UpdateDribbleLoop(false);

            if (IsServer)
            {
                _isRolling.Value = false;
                PlayBrokenSfxClientRpc();
            }

            Vector3 shatterOrigin = transform.position;

            _capsuleCollider.enabled = false;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;

            for (int i = 0; i < _childRigidbodies.Length; i++)
            {
                Rigidbody childRigidbody = _childRigidbodies[i];
                if (childRigidbody == _rigidbody)
                {
                    continue;
                }

                childRigidbody.isKinematic = false;
                childRigidbody.useGravity = true;
                childRigidbody.detectCollisions = true;
                childRigidbody.linearVelocity = rootVelocity;

                if (rootVelocity.sqrMagnitude > Mathf.Epsilon)
                {
                    ApplyShatterImpulse(childRigidbody, shatterOrigin);
                }
            }

            for (int i = 0; i < _childColliders.Length; i++)
            {
                Collider childCollider = _childColliders[i];
                if (childCollider == _capsuleCollider)
                {
                    continue;
                }

                childCollider.enabled = true;
            }

            _shardShrink.Play();

            if (IsHost)
            {
                _resourcesServices.DestroyObject(gameObject, ShatterReturnDelay);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ExplodeShatterClientRpc()
        {
            if (_hasShattered)
            {
                return;
            }

            EnterShatterState(GetLocalShatterRootVelocity());
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PlayBrokenSfxClientRpc()
        {
            UpdateDribbleLoop(false);
            _soundPlayerBinder.PlayDetached(BrokenCueId);
        }

        private void ApplyShatterImpulse(Rigidbody childRigidbody, Vector3 shatterOrigin)
        {
            Vector3 shatterDirection = childRigidbody.worldCenterOfMass - shatterOrigin;
            shatterDirection.y = Mathf.Max(shatterDirection.y, 0f) + ShatterUpwardBias;

            if (shatterDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                shatterDirection = Random.insideUnitSphere;
                shatterDirection.y = Mathf.Abs(shatterDirection.y) + ShatterUpwardBias;
            }

            childRigidbody.AddForce(
                shatterDirection.normalized * ShatterImpulseForce,
                ForceMode.Impulse);
        }

        private void SpawnShatterVfx(Vector3 spawnPosition)
        {
            _vfxManagerServices.InstantiateParticleInArea(MinionRockVfxPath, spawnPosition);
        }

        private void ApplyPlayerHit(Collision collision)
        {
            if (collision.transform.TryGetComponentInParents(out PlayerStats playerStats) == false)
            {
                throw new MissingComponentException(
                    $"[{nameof(StoneGolemRollingRockController)}] Player collision target is missing {nameof(PlayerStats)}.");
            }

            if (playerStats.IsDead)
            {
                return;
            }

            playerStats.OnAttacked(_minionRockStats, _minionRockStats.Attack);

            if (playerStats.IsDead == false)
            {
                ApplyPlayerStun(playerStats);
                ApplyPlayerPush(playerStats);
            }
        }

        private void ApplyPlayerStun(PlayerStats playerStats)
        {
            if (playerStats.TryGetComponent(out ICCReceiver crowdControlReceiver) == false)
            {
                throw new MissingComponentException(
                    $"[{nameof(StoneGolemRollingRockController)}] {playerStats.gameObject.name} is missing {nameof(ICCReceiver)}.");
            }

            HeadTr headTr = playerStats.GetComponentInChildren<HeadTr>();
            if (headTr == null)
            {
                throw new MissingComponentException(
                    $"[{nameof(StoneGolemRollingRockController)}] {playerStats.gameObject.name} is missing {nameof(HeadTr)}.");
            }

            crowdControlReceiver.ApplyCC(CCType.Stun, gameObject, PlayerStunDuration);

            _vfxManagerServices.InstantiateParticleWithTarget(
                StunVfxPath,
                headTr.transform,
                PlayerStunDuration,
                true);
        }

        private void ApplyPlayerPush(PlayerStats playerStats)
        {
            if (playerStats.transform.TryGetComponentInParents(out PlayerInitializeNgo playerInitializeNgo) == false)
            {
                throw new MissingComponentException(
                    $"[{nameof(StoneGolemRollingRockController)}] {playerStats.gameObject.name} is missing {nameof(PlayerInitializeNgo)}.");
            }

            Vector3 pushDir = playerStats.transform.position - transform.position;
            pushDir.y = 0f;

            if (pushDir.sqrMagnitude <= 0.0001f)
            {
                pushDir = _moveDirection.Value.sqrMagnitude > Mathf.Epsilon
                    ? _moveDirection.Value
                    : transform.forward;
            }
            else
            {
                pushDir.Normalize();
            }

            playerInitializeNgo.PushBackFromNetworkRpc(
                pushDir,
                PlayerPushDistance,
                PlayerPushDuration);
        }

        private Vector3 ResolveGroundPosition(Vector3 targetPosition)
        {
            if (_groundLayerMask == 0)
            {
                return targetPosition;
            }

            float bottomOffset = GetCapsuleBottomOffset();
            Vector3 rayOrigin = targetPosition + Vector3.up * (bottomOffset + GroundRayExtraHeight);
            float rayDistance = bottomOffset + GroundRayDistance;

            if (Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out RaycastHit hit,
                    rayDistance,
                    _groundLayerMask,
                    QueryTriggerInteraction.Ignore))
            {
                targetPosition.y = hit.point.y + bottomOffset + GroundSnapPadding;
            }

            return targetPosition;
        }

        private float GetCapsuleBottomOffset()
        {
            if (_capsuleCollider == null)
            {
                return 0f;
            }

            Vector3 lossyScale = transform.lossyScale;
            float scaledRadius = _capsuleCollider.radius;
            float scaledHeight = _capsuleCollider.height;
            float scaledCenterY = _capsuleCollider.center.y;

            switch (_capsuleCollider.direction)
            {
                case 0:
                    scaledRadius *= Mathf.Max(Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
                    scaledHeight *= Mathf.Abs(lossyScale.x);
                    scaledCenterY *= lossyScale.y;
                    break;
                case 2:
                    scaledRadius *= Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
                    scaledHeight *= Mathf.Abs(lossyScale.z);
                    scaledCenterY *= lossyScale.y;
                    break;
                default:
                    scaledRadius *= Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.z));
                    scaledHeight *= Mathf.Abs(lossyScale.y);
                    scaledCenterY *= lossyScale.y;
                    break;
            }

            float cylinderHalf = Mathf.Max(0f, (scaledHeight * 0.5f) - scaledRadius);
            return Mathf.Max(0f, scaledCenterY + cylinderHalf + scaledRadius);
        }

        private void RotateVisualChildren(Vector3 moveDirection, float moveSpeed, float deltaTime)
        {
            if (moveDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Vector3 rotationAxis = Vector3.Cross(Vector3.up, moveDirection.normalized);
            if (rotationAxis.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float rotationDegrees = moveSpeed * ClientRotationDegreesPerSecondAtUnitSpeed * deltaTime;
            Vector3 pivot = transform.position;

            for (int i = 0; i < _visualChildren.Length; i++)
            {
                Transform visualChild = _visualChildren[i];
                if (visualChild == transform)
                {
                    continue;
                }

                visualChild.RotateAround(pivot, rotationAxis, rotationDegrees);
            }
        }

        private bool IsWallLayer(int layer)
        {
            return (_wallLayerMask & (1 << layer)) != 0;
        }

        private bool IsPlayerLayer(int layer)
        {
            return (_playerLayerMask & (1 << layer)) != 0;
        }

        private void UpdateDribbleLoop(bool isRolling)
        {
            if (isRolling && _hasShattered == false)
            {
                _soundPlayerBinder.PlayLoop(DribbleCueId);
                return;
            }

            _soundPlayerBinder.StopLoop(DribbleCueId);
        }
    }
}
