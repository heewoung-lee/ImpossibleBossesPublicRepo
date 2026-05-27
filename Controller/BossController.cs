using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Controller.ControllerStats;
using GameManagers.RelayManagement;
using NetWork;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace Controller
{
    public abstract class BossController : BaseController
    {
        [Inject]  private RelayManager _relayManager;
        private BehaviorTree _behaviorTree;

        private GameObject _targetObjectInBehaviourTree;
        public GameObject TargetObjectInBehaviourTree
        {
            get
            {
                if (_behaviorTree != null)
                {
                    var sharedTarget = _behaviorTree.GetVariable("Target") as SharedGameObject;
                    return sharedTarget?.Value; 
                }
                return _targetObjectInBehaviourTree; // BT가 없다면 필드값 반환
            }
            set
            {
                _targetObjectInBehaviourTree = value;
                if (_behaviorTree != null)
                {
                    _behaviorTree.SetVariableValue("Target", value);
                }
            }
        }

        private bool _isTauntedInBehaviourTree;
        public bool IsTauntedInBehaviourTree
        {
            get
            { 
                if (_behaviorTree != null)
                {
                    var sharedTarget = _behaviorTree.GetVariable("IsTaunted") as SharedGameObject;
                    return sharedTarget?.Value; 
                }
                return _isTauntedInBehaviourTree; // BT가 없다면 필드값 반환
            }
            set
            {
                _isTauntedInBehaviourTree = value;
                if (_behaviorTree != null)
                {
                    _behaviorTree.SetVariableValue("IsTaunted", value);
                }
            } 
        }



        public override Define.WorldObject WorldobjectType { get; protected set; } = Define.WorldObject.Boss;
        
        //상태별 애니메이션 정지 타이밍 비율을 들고 있는 딕셔너리
        //해당 상태일때 어드 선에서 멈춰라 라는걸 들고 있음.
        //이로인해 골렘의 펀치모션 등의 묵직함을 연출할 수 있음
        public abstract Dictionary<IState, float> StateDecelerationRatioDict { get; }
        
        
        
        
        private BossAnimationNetworkController _networkController;
        protected override void AwakeInit()
        {
            _networkController = GetComponent<BossAnimationNetworkController>();
            _behaviorTree = GetComponent<BehaviorTree>();
        }
        

        protected override void OnStateChanged(IState newState)
        {
            base.OnStateChanged(newState);
            if (_networkController != null) // 만약 상태가 달라져 애니메이션을 바꿔야 한다면 바뀐 애니메이션을 전파
            {
                SyncBossStateToClients(newState);
            }
        }
        private void SyncBossStateToClients<T>(T state) where T : IState
        {
            if (_relayManager.NetworkManagerEx.IsHost == false)
                return;
            
            string typename = state.GetType().Name;
            
            _networkController.SyncAnimationState(typename);
        }

        public void SyncCurrentAnimationSpeedOverride(float animationSpeed)
        {
            if (_networkController == null || _relayManager.NetworkManagerEx.IsHost == false)
            {
                return;
            }

            _networkController.SyncCurrentAnimationSpeedOverride(animationSpeed);
        }

        public bool TryGetAnimationSpeed(double elapsedTime, out float animSpeed, NetworkAnimationInfo animinfo,bool isCheckattackIndicatorFinish)
        {
            animSpeed = 0f;
            float startAnimSpeed =animinfo.StartAnimationSpeed;
            animSpeed = Mathf.Lerp(startAnimSpeed, 0f, (float)(elapsedTime / (animinfo.AnimLength * animinfo.DecelerationRatio)));
            Anim.speed = animSpeed;
            bool finished = animSpeed <= animinfo.AnimStopThreshold&& isCheckattackIndicatorFinish == true;
            if (finished)
            {
                animSpeed = startAnimSpeed;
                Anim.speed = animSpeed;
            }
            return finished;
        }



        public bool TryGetAttackTypePreTime(IState attackType,out float preTime)
        {
            if (StateDecelerationRatioDict.TryGetValue(attackType, out float preTimetoDict) == false)
            {
                UtilDebug.LogError($"Attack type {attackType} not found in AttackPreFrameDict.");
                preTime = default;
                return false;
            }
            preTime = preTimetoDict;
            return true;
        }
    }
}
