using System;
using System.Collections;
using System.Threading;
using BehaviorDesigner.Runtime;
using Controller;
using Controller.CrowdControl;
using Controller.BossState.BossGolem;
using Cysharp.Threading.Tasks;
using DataType.Skill.Factory.Effect.Def;
using GameManagers.GameManagerExManagement;
using GameManagers.RelayManagement;
using NetWork.BaseNGO;
using NetWork.Sync;
using Unity.Netcode;
using UnityEngine;
using Util;
using VFX;
using Zenject;

namespace NetWork
{
    public class NGOBossNetworkController : NetworkBehaviourBase, ICCReceiver
    {
        [Inject] protected IBossSpawnManager _bossSpawnManager;
        [Inject] protected RelayManager _relayManager;
        protected BossController _bossController;

        private readonly float _maxAnimSpeed = 3f;
        private readonly float _catchUpDuration = 0.2f;

        private CancellationTokenSource _tauntTokenSource; // 도발 취소용 토큰
       
        private bool _finishedAttack = false;
        private Coroutine _animationCoroutine;
        private bool _finishedIndicatorDuration = false;
        private bool _isCatchUpInitialized = false;
        private float _remainingCatchUpTime = 0f;

        public bool FinishAttack
        {
            get => _finishedAttack;
            private set => _finishedAttack = value;
        }

        protected override void AwakeInit()
        {
            _bossController = GetComponent<BossController>();
        }

        protected override void StartInit()
        {
        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _bossSpawnManager.SetBossMonster(gameObject);
            if (IsHost == false)
            {
                InitBossOnClient();
            }

            void InitBossOnClient()
            {
                GetComponent<BossController>().enabled = false;
                GetComponent<BehaviorTree>().enabled = false;
            }
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SetTargetServerRpc(NetworkObjectReference targetRef)
        {
            if (targetRef.TryGet(out NetworkObject targetNetObj))
            {
                _bossController.TargetObjectInBehaviourTree = targetNetObj.gameObject;
            }
        }
        
        
        
        public void ApplyCC(CCType ccType, GameObject caster, float duration)
        {
            if (ccType == CCType.Root)
            {
                return;
            }

            if (ccType == CCType.Taunt)
            {
                if (caster.TryGetComponent(out NetworkObject netObj))
                {
                    //누가, 얼마 동안 도발했는지 서버에 알립니다.
                    ApplyTauntServerRpc(netObj, duration);
                }
            }
        }
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ApplyTauntServerRpc(NetworkObjectReference targetRef, float duration)
        {
            //서버에서만 실행되는 코드입니다.
            if (targetRef.TryGet(out NetworkObject targetNetObj))
            {
                //기존 도발 타이머(서버용) 취소 및 갱신
                _tauntTokenSource?.Cancel();
                _tauntTokenSource?.Dispose();
                _tauntTokenSource = new CancellationTokenSource();

                //서버 측 타이머 실행
                ExecuteTauntOnServerAsync(targetNetObj, duration, _tauntTokenSource.Token).Forget();
            }
        }
        private async UniTaskVoid ExecuteTauntOnServerAsync(NetworkObject target, float duration, CancellationToken token)
        {
            try
            {
                // 서버의 BossController 상태 직접 변경 (이 값이 비헤이비어 트리에 반영됨)
                _bossController.TargetObjectInBehaviourTree = target.gameObject;
                _bossController.IsTauntedInBehaviourTree = true;
        
                UtilDebug.Log($"[Server] 도발 적용됨: {target.name} ({duration}초)");

                // 서버 시간 기준으로 대기
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);

                // 도발 종료 처리
                _bossController.IsTauntedInBehaviourTree = false;
                _bossController.TargetObjectInBehaviourTree = null;
        
                UtilDebug.Log("[Server] 도발 종료 - 타겟 초기화");
            }
            catch (OperationCanceledException)
            {
                // 새로운 도발(ApplyTauntServerRpc)이 들어와서 취소된 경우
                UtilDebug.Log("[Server] 도발 갱신: 이전 타이머 취소");
            }
        }
        
        
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            _tauntTokenSource?.Cancel();
            _tauntTokenSource?.Dispose();
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void StartAnimChangedRpc(NetworkAnimationInfo animinfo, NetworkObjectReference indicatorRef = default)
        {
            NgoIndicatorController indicatorController = null;
            if (indicatorRef.Equals(default) == false)
            {
                if (indicatorRef.TryGet(out NetworkObject ngo))
                {
                    indicatorController = ngo.GetComponent<NgoIndicatorController>();
                }
            }

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
            
            _isCatchUpInitialized = false;
            _remainingCatchUpTime = 0f;

            if (animinfo.AddIndicatorDuration < 0f) // faster swing
            {
                float speedScale = animinfo.AnimLength /
                                   Mathf.Max(animinfo.AnimLength + animinfo.AddIndicatorDuration, 0.1f);
                animinfo.StartAnimationSpeed *= speedScale;
                animinfo.AnimStopThreshold *= speedScale;
            }

            _bossController.Anim.speed = animinfo.StartAnimationSpeed;
            _animationCoroutine = StartCoroutine(UpdateAnimCorutine(animinfo, indicatorController));
        }

        IEnumerator UpdateAnimCorutine(NetworkAnimationInfo animinfo, NgoIndicatorController indicatorCon = null)
        {
            double elaspedTime = 0f;
            FinishAttack = false;
            _finishedIndicatorDuration = false;
            double nowTime = _relayManager.NetworkManagerEx.ServerTime.Time;

            if (indicatorCon != null)
            {
                indicatorCon.OnIndicatorDone += () => { _finishedIndicatorDuration = true; };
            }
            else
            {
                StartCoroutine(UpdateIndicatorDurationTime(animinfo.AddIndicatorDuration, animinfo.AnimLength,
                    nowTime));
            }

            while (elaspedTime <= animinfo.AnimLength)
            {
                double currentNetTime = _relayManager.NetworkManagerEx.ServerTime.Time;
                double deltaTime = (currentNetTime - nowTime);
                nowTime = currentNetTime;

                //여기서 현재 인디케이터가 다 마쳤는지 확인해야함
                bool finished = _bossController.TryGetAnimationSpeed(
                    elaspedTime,
                    out float animspeed,
                    animinfo,
                    _finishedIndicatorDuration);

                float speedMultiplier = finished
                    ? 1f
                    : AnimationCatchUpCalculator.ConsumeSpeedMultiplier(
                        _relayManager.NetworkManagerEx,
                        animinfo.ServerTime,
                        ref _isCatchUpInitialized,
                        ref _remainingCatchUpTime,
                        (float)deltaTime,
                        _catchUpDuration,
                        _maxAnimSpeed);

                _bossController.Anim.speed = animspeed * speedMultiplier;
                elaspedTime += deltaTime * animspeed * speedMultiplier;
                

                yield return null;
            }

            FinishAttack = true;
            _bossController.Anim.speed = 1;
        }

        //'서버의 절대 시간'을 기준으로 인디케이터의 유지 시간과 종료 싱크를 완벽하게 맞추기 위한 함수
        IEnumerator UpdateIndicatorDurationTime(float indicatorAddduration, float animLength, double prevNetTime)
        {
            _finishedIndicatorDuration = false;
            float elapsedTime = 0f;
            while (elapsedTime <= indicatorAddduration + animLength)
            {
                double currentNetTime = _relayManager.NetworkManagerEx.ServerTime.Time;
                float deltaTime = (float)(currentNetTime - prevNetTime);
                elapsedTime += deltaTime;

                //다음 프레임 계산을 위해 기준 시간을 방금 시간으로 갱신
                prevNetTime = currentNetTime;

                yield return null;
            }

            _finishedIndicatorDuration = true;
        }


    }
}
