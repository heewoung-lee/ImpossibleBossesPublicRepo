using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using Controller.BossState.BossRedDragon;
using Data;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using Module.EnemyModule.Boss.RedDragon;
using NetWork.BossRedDragon_NGO;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Util;
using VFX;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonBreath : Action
    {
        private const string IndicatorPath = "Prefabs/Enemy/Boss/Indicator/NGODragonBreathIndicator";
        private const string BreathVfxPath = "Prefabs/Enemy/Boss/AttackPattern/RedDragon/DragonBreathVFX";
        private const string BreathStartClipName = "RedDragonBreathStart";
        private const float AddIndicatorDurationTime = 0f;
        private const bool CheckDuplicateTargetPerTick = true;

        private enum BreathPhase
        {
            Start,
            Loop,
            End
        }

        [SerializeField] private float _loopDuration = 2f;
        [SerializeField] private SharedProjector _breathIndicator;
        [SerializeField] private int _damage = -1;
        [SerializeField] private float _colliderEnableDelay = 0.1f;
        [SerializeField] private float _damageInterval = 0.2f;

        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        private IAttackRange _attackRange;
        private BossRedDragonController _controller;
        private NGOBossRedDragonNetworkController _networkController;
        private NgoArrowIndicatorController _breathIndicatorController;
        private RedDragonBreathDamageDealer _breathDamageDealer;
        private RedDragonSoundAnimationEvent _soundAnimationEvent;
        private BreathPhase _phase;
        private float _loopEndTime;
        private float _breathStartAnimLength;
        private GameObject _breathVfxPrefab;

        private IResourcesServices ResourcesServices
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

        private RelayManager RelayManager
        {
            get
            {
                if (_relayManager == null)
                {
                    _relayManager = GetComponent<BossDependencyHub>().RelayManager;
                }

                return _relayManager;
            }
        }

        public override void OnAwake()
        {
            base.OnAwake();
            _attackRange = Owner.GetComponent<IAttackRange>();
            _controller = Owner.GetComponent<BossRedDragonController>();
            _networkController = Owner.GetComponent<NGOBossRedDragonNetworkController>();
            _soundAnimationEvent = Owner.GetComponent<RedDragonSoundAnimationEvent>();
            _breathStartAnimLength = Utill.GetAnimationLength(BreathStartClipName, _controller.Anim);
            _breathVfxPrefab = ResourcesServices.Load<GameObject>(BreathVfxPath);
            CacheBreathDamageDealer();
        }

        public override void OnStart()
        {
            base.OnStart();
            _phase = BreathPhase.Start;
            ConfigureBreathDamageDealer();
            _networkController.SetBreathLoopState(false);
            _controller.UpdateBreathStart();
            SpawnBreathIndicator();
            StartBreathStartAnimationByIndicatorDuration();
        }

        public override TaskStatus OnUpdate()
        {
            switch (_phase)
            {
                case BreathPhase.Start:
                    if (_controller.IsAnimationDone(BossRedDragonAnimHash.RedDragonBreathStart))
                    {
                        _phase = BreathPhase.Loop;
                        _loopEndTime = Time.time + _loopDuration;
                        _networkController.SetBreathLoopState(true);
                        _soundAnimationEvent?.PlayBreathSfxFromNode(_loopDuration);
                        SpawnBreathVfx();
                        _controller.UpdateBreathLoop();
                    }

                    return TaskStatus.Running;

                case BreathPhase.Loop:
                    if (Time.time >= _loopEndTime)
                    {
                        _phase = BreathPhase.End;
                        _networkController.SetBreathLoopState(false);
                        _controller.UpdateBreathEnd();
                    }

                    return TaskStatus.Running;

                case BreathPhase.End:
                    return _controller.IsAnimationDone(BossRedDragonAnimHash.RedDragonBreathEnd)
                        ? TaskStatus.Success
                        : TaskStatus.Running;

                default:
                    return TaskStatus.Running;
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _networkController.SetBreathLoopState(false);
            _breathIndicatorController = null;
            _controller.UpdateIdle();
        }

        private void SpawnBreathIndicator()
        {
            GameObject indicatorObject = ResourcesServices.InstantiateByKey(IndicatorPath);
            indicatorObject = RelayManager.SpawnNetworkObj(indicatorObject);
            _breathIndicatorController = indicatorObject.GetComponent<NgoArrowIndicatorController>();

            if (_breathIndicatorController == null)
            {
                return;
            }

            _breathIndicator.Value = _breathIndicatorController;
            if (Owner.TryGetComponent(out NetworkObject ownerNetworkObject))
            {
                _breathIndicatorController.SetSpawnerBossNetworkObjectId(ownerNetworkObject.NetworkObjectId);
            }

            float totalIndicatorDurationTime = AddIndicatorDurationTime + _breathStartAnimLength;
            _breathIndicatorController.PlayWithCurrentShape(_controller.transform, totalIndicatorDurationTime);
        }

        private void StartBreathStartAnimationByIndicatorDuration()
        {
            float indicatorDuration = AddIndicatorDurationTime + _breathStartAnimLength;
            if (indicatorDuration <= Mathf.Epsilon)
            {
                return;
            }

            float animationSpeed = _breathStartAnimLength / indicatorDuration;
            _networkController.StartBreathStartAnimRpc(
                _breathStartAnimLength,
                animationSpeed,
                RelayManager.NetworkManagerEx.ServerTime.Time);
        }

        private void SpawnBreathVfx()
        {
            Transform breathAttachTransform = _networkController.GetBreathAttachTransform();
            Vector3 basePosition = breathAttachTransform != null
                ? breathAttachTransform.position
                : _controller.transform.position;
            Quaternion baseRotation = breathAttachTransform != null
                ? breathAttachTransform.rotation
                : _controller.transform.rotation;
            Vector3 prefabPositionOffset = _breathVfxPrefab != null
                ? _breathVfxPrefab.transform.localPosition
                : Vector3.zero;
            Quaternion prefabRotationOffset = _breathVfxPrefab != null
                ? _breathVfxPrefab.transform.rotation
                : Quaternion.identity;
            Vector3 spawnPosition = basePosition + baseRotation * prefabPositionOffset;
            Quaternion spawnRotation = baseRotation * prefabRotationOffset;
            Vector3 spawnScale = _breathVfxPrefab != null
                ? _breathVfxPrefab.transform.localScale
                : Vector3.one;

            RelayManager.NgoRPCCaller.SpawnVFXPrefabServerRpc(
                BreathVfxPath,
                _loopDuration,
                spawnPosition,
                spawnRotation,
                spawnScale,
                default);
        }

        private void CacheBreathDamageDealer()
        {
            if (_breathDamageDealer != null)
            {
                return;
            }

            Transform breathAttachTransform = _networkController.GetBreathAttachTransform();
            if (breathAttachTransform != null)
            {
                _breathDamageDealer = breathAttachTransform.GetComponent<RedDragonBreathDamageDealer>();
            }
        }

        private void ConfigureBreathDamageDealer()
        {
            CacheBreathDamageDealer();
            if (_breathDamageDealer == null || _attackRange == null)
            {
                return;
            }

            _breathDamageDealer.Configure(
                _attackRange,
                _damage,
                _colliderEnableDelay,
                CheckDuplicateTargetPerTick,
                _damageInterval);
        }
    }
}
