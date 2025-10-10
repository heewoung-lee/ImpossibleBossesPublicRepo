using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Controller.BossState;
using Controller.ControllerStats;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using NetWork;
using NetWork.Boss_NGO;
using NetWork.NGO.Interface;
using Stats.BaseStats;
using Stats.BossStats;
using UnityEngine;
using Util;
using VFX;
using Zenject;

namespace BehaviourTreeNode.BossGolem.Task
{
    public class BossAttack : Action, IBossAnimationChanged
    {
        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        
        public IResourcesServices ResourcesServicesInstance
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
        
        public RelayManager RelayManagerInstance
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
        
        
        [SerializeField] private SharedProjector _attackIndicator;
        [SerializeField] private int _radiusStep = 0;
        [SerializeField] private int _angleStep = 0;
        
        private readonly float _addIndicatorAddDurationTime = 0f;
        private readonly float _attackAnimStopThreshold = 0.06f;

        private BossGolemController _controller;
        private BossGolemNetworkController _networkController;
        private float _animLength = 0f;
        private List<Vector3> _attackRangeParticlePos;
        private BossStats _stats;
        private bool _hasSpawnedParticles;

        private NgoIndicatorController _indicatorController;
        private BossGolemAnimationNetworkController _bossGolemAnimationNetworkController;

        
        public BossGolemAnimationNetworkController BossAnimNetworkController => _bossGolemAnimationNetworkController;

        public override void OnAwake()
        {
            base.OnAwake();
            
            
            
            ChechedBossAttackField();

            void ChechedBossAttackField()
            {
                _controller = Owner.GetComponent<BossGolemController>();
                _stats = _controller.GetComponent<BossStats>();
                _animLength = Utill.GetAnimationLength("Anim_Attack1", _controller.Anim);
                _networkController = Owner.GetComponent<BossGolemNetworkController>();
                _bossGolemAnimationNetworkController = Owner.GetComponent<BossGolemAnimationNetworkController>();
            }
        }


        public override void OnStart()
        {
            base.OnStart();
            SpawnAttackIndicator();
            CalculateBossAttackRange();
            StartAnimationSpeedChanged();

            void SpawnAttackIndicator()
            {
                OnBossGolemAnimationChanged(BossAnimNetworkController, _controller.BaseAttackState);
                _hasSpawnedParticles = false;
                _indicatorController = ResourcesServicesInstance.
                    InstantiateByKey("Prefabs/Enemy/Boss/Indicator/Boss_Attack_Indicator")
                    .GetComponent<NgoIndicatorController>();
                _attackIndicator.Value = _indicatorController;
                _indicatorController = RelayManagerInstance.SpawnNetworkObj(_indicatorController.gameObject)
                    .GetComponent<NgoIndicatorController>();
                float totalIndicatorDurationTime = _addIndicatorAddDurationTime + _animLength;
                _indicatorController.SetValue(_stats.ViewDistance, _stats.ViewAngle, _controller.transform,
                    totalIndicatorDurationTime, IndicatorDoneEvent);

                void IndicatorDoneEvent()
                {
                    if (_hasSpawnedParticles) return;
                    string dustPath = "Prefabs/Particle/AttackEffect/Dust_Particle";
                    SpawnParamBase param = SpawnParamBase.Create(argFloat: 1f);
                    RelayManagerInstance.NgoRPCCaller.SpawnNonNetworkObject(_attackRangeParticlePos, dustPath,
                        param);

                    #region 5.6일 파티클 스폰방식 수정

                    //foreach (var pos in _attackRangeParticlePos)
                    //{
                    //    Managers.VFX_Manager.GenerateParticle("Prefabs/Particle/AttackEffect/Dust_Particle", pos, 1f);
                    //} 5.6일 Update 이전 파티클들은 네트워크 스폰 + 네트워크 오브젝트 풀링으로 최적화를 했는데
                    // 많은 오브젝트 풀링을 네트워크로 하다보니, 네트워크에 과부하가 걸림
                    // 해결방반으로 RPC_Caller에게 스폰할 오브젝트 경로,위치,공통 파라미터만 보내고
                    // RPC_Caller는 ISpawnBehavior인터페이스를 상속받은 오브젝트를 스폰하게끔 수정

                    #endregion

                    TargetInSight.AttackTargetInSector(_stats);
                    _hasSpawnedParticles = true;
                }
            }

            void CalculateBossAttackRange()
            {
                _attackRangeParticlePos = TargetInSight.GeneratePositionsInSector(_controller.transform,
                    _controller.GetComponent<IAttackRange>().ViewAngle,
                    _controller.GetComponent<IAttackRange>().ViewDistance,
                    _angleStep, _radiusStep);
            }

            void StartAnimationSpeedChanged()
            {
                if (_controller.TryGetAttackTypePreTime(_controller.BaseAttackState, out float decelerationRatio) is
                    false)
                    return;


                CurrentAnimInfo animInfo = new CurrentAnimInfo(_animLength, decelerationRatio, _attackAnimStopThreshold,
                    _addIndicatorAddDurationTime, RelayManagerInstance.NetworkManagerEx.ServerTime.Time);
                _networkController.StartAnimChagnedRpc(animInfo,
                    RelayManagerInstance.GetNetworkObject(_indicatorController.gameObject));
                //호스트가 pretime 뽑아서 모든 클라이언트 들에게 던져야함.
            }
        }


        public override TaskStatus OnUpdate()
        {
            return _networkController.FinishAttack == true ? TaskStatus.Success : TaskStatus.Running;
        }


        public override void OnEnd()
        {
            base.OnEnd();
            _attackRangeParticlePos = null;
            _hasSpawnedParticles = false;
        }

        public void OnBossGolemAnimationChanged(BossGolemAnimationNetworkController bossAnimController, IState state)
        {
            bossAnimController.SyncBossStateToClients(state);
        }
    }
}