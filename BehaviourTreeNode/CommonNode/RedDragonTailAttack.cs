using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using Controller.BossState.BossRedDragon;
using Data;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using Module.EnemyModule.Boss.RedDragon;
using NetWork;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Util;
using VFX;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonTailAttack : Action
    {
        private const string IndicatorPath = "Prefabs/Enemy/Boss/Indicator/RedDragonAttackIndicator";

        private const string DustPath = "Prefabs/Enemy/Boss/AttackPattern/RedDragon/RedDragonTailAttackVFX";
        private const string TailAttackClipName = "RedDragonTailAttack";
        private const float IndicatorCompleteAnimationRatio = 0.35f;

        [SerializeField] private SharedProjector _attackIndicator;
        [SerializeField] private int _radiusStep;
        [SerializeField] private int _angleStep;

        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        private BossRedDragonController _controller;
        private RedDragonSoundAnimationEvent _soundAnimationEvent;
        private IAttackRange _attackRange;
        private NgoIndicatorController _indicatorController;
        private List<Vector3> _attackRangeParticlePositions;
        private float _animLength;
        private bool _hasSpawnedParticles;

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
            _controller = Owner.GetComponent<BossRedDragonController>();
            _soundAnimationEvent = Owner.GetComponent<RedDragonSoundAnimationEvent>();
            _attackRange = Owner.GetComponent(typeof(IAttackRange)) as IAttackRange;
            _animLength = Utill.GetAnimationLength(TailAttackClipName, _controller.Anim);
        }

        public override void OnStart()
        {
            base.OnStart();
            _hasSpawnedParticles = false;
            SpawnAttackIndicator();
            _controller.UpdateTailAttack();
            CalculateAttackRange();
        }

        public override TaskStatus OnUpdate()
        {
            return _controller.IsAnimationDone(BossRedDragonAnimHash.RedDragonTailAttack)
                ? TaskStatus.Success
                : TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _attackRangeParticlePositions = null;
            _hasSpawnedParticles = false;
            _controller.UpdateIdle();
        }

        private void SpawnAttackIndicator()
        {
            GameObject indicatorObject = ResourcesServices.InstantiateByKey(IndicatorPath);
            _indicatorController = indicatorObject.GetComponent<NgoIndicatorController>();
            if (_indicatorController == null)
            {
                UtilDebug.LogError($"[{nameof(RedDragonTailAttack)}] {nameof(NgoIndicatorController)} is missing.");
                return;
            }

            _attackIndicator.Value = _indicatorController;
            _indicatorController = RelayManager.SpawnNetworkObj(_indicatorController.gameObject)
                .GetComponent<NgoIndicatorController>();
            if (Owner.TryGetComponent(out NetworkObject ownerNetworkObject))
            {
                _indicatorController.SetSpawnerBossNetworkObjectId(ownerNetworkObject.NetworkObjectId);
            }

            float totalIndicatorDurationTime = _animLength * IndicatorCompleteAnimationRatio;
            _indicatorController.SetValue(
                _attackRange.ViewDistance,
                _attackRange.ViewAngle,
                _controller.transform,
                totalIndicatorDurationTime,
                OnIndicatorDone);
        }

        private void OnIndicatorDone()
        {
            if (_hasSpawnedParticles)
            {
                return;
            }

            _soundAnimationEvent.PlayTailAttackSfxFromNode();
            SpawnTailAttackVfx();
            TargetInSight.AttackTargetInSector(_attackRange);
            _hasSpawnedParticles = true;
        }

        private void CalculateAttackRange()
        {
            _attackRangeParticlePositions = TargetInSight.GeneratePositionsInSector(
                _controller.transform,
                _attackRange.ViewAngle,
                _attackRange.ViewDistance,
                _angleStep,
                _radiusStep);
        }

        private void SpawnTailAttackVfx()
        {
            if (_attackRangeParticlePositions == null || _attackRangeParticlePositions.Count == 0)
            {
                return;
            }

            NetworkParams networkParams = new NetworkParams(argFloat: 1f);
            RelayManager.NgoRPCCaller.SpawnNonNetworkObject(_attackRangeParticlePositions, DustPath, networkParams);
        }
    }
}
