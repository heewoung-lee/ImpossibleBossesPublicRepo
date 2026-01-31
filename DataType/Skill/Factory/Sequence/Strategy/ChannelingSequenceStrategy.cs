using System;
using System.Threading;
using Controller;
using Controller.ControllerStats.BaseStates;
using Cysharp.Threading.Tasks;
using DataType.Skill.Factory.Decorator;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Effect;
using DataType.Skill.Factory.Sequence.Def;
using DataType.Skill.Factory.Sequence.GetLength.Strategy;
using DataType.Skill.Factory.Target;
using DataType.Skill.ShareDataDef;
using Module.PlayerModule.PlayerClassModule;
using Skill;
using Stats.BaseStats;
using UnityEngine;

namespace DataType.Skill.Factory.Sequence.Strategy
{
    /// <summary>
    /// 채널링 전략은 
    /// </summary>
    public sealed class ChannelingSequenceStrategy : ISequenceStrategy
    {
        public Type DefType => typeof(ChannelingSequenceDef);

        private readonly IMeleeComboLengthResolver _lengthResolver;

        public ChannelingSequenceStrategy(IMeleeComboLengthResolver lengthResolver)
        {
            _lengthResolver = lengthResolver;
        }

        public ISequenceModule Create(ISequenceDef def, BaseController owner)
            => new Module((ChannelingSequenceDef)def, _lengthResolver);


        private sealed class Module : ISequenceModule
        {
            private readonly ChannelingSequenceDef _def;
            private readonly IMeleeComboLengthResolver _lengthResolver;
            private CancellationTokenSource _cts;
            private readonly InterruptFlag _interrupt;
            private int _animHash = -1;

            public Module(ChannelingSequenceDef def, IMeleeComboLengthResolver lengthResolver)
            {
                _def = def;
                _lengthResolver = lengthResolver;
                _interrupt = _def.interruptFlag;
            }


            public void Execute(
                SkillExecutionContext ctx,
                ITargetingModule targeting,
                IDecoratorModule decorator,
                IEffectModule effect,
                Action onComplete,
                Action onCancel)
            {
                CancelRunning();
                CancellationTokenSource runCts = new CancellationTokenSource();
                _cts = runCts; //비동기 도중에 Excute가 두번 들어오는 경우가 있으므로 안전하게 RunAsync에 토큰을 넘김


                RunAsync(ctx, targeting, decorator, effect, onComplete, onCancel, runCts).Forget();
            }

            public void Release()
            {
                CancelRunning();
            }

            private void CancelRunning()
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                }
            }

            private async UniTaskVoid RunAsync(
                SkillExecutionContext ctx,
                ITargetingModule targeting,
                IDecoratorModule decorator,
                IEffectModule effect,
                Action onComplete,
                Action onCancel,
                CancellationTokenSource runCts)
            {
                if (ctx == null || targeting == null || decorator == null || effect == null)
                {
                    if (onCancel != null) onCancel();
                    return;
                }

                if (string.IsNullOrEmpty(ctx.SkillData.animationStateName))
                {
                    Debug.LogError(
                        $"[ChannelingStrategy] Animation State Name is missing in SkillData: {ctx.SkillData.name}");
                    onCancel?.Invoke();
                    return;
                }

                bool finished = false;
                float currentTime = Time.time;

                IDamageable casterDamageable = ctx.Caster.GetComponent<IDamageable>();

                ModulePlayerClass playerModule = ctx.Caster.GetComponent<ModulePlayerClass>();
                Debug.Assert(playerModule != null, nameof(playerModule) + " ModulePlayerClass not found");

                BaseController playerBaseController = ctx.Caster.GetComponent<BaseController>();
                Debug.Assert(playerBaseController != null, nameof(playerBaseController) + " BaseController not found");


                CancellationToken destroyToken = CancellationToken.None;
                if (ctx.Caster != null)
                    destroyToken = ctx.Caster.GetCancellationTokenOnDestroy();

                //ctx토큰과 캐스터의 토큰 둘중 하나만 사라져도 멈추게 작동
                CancellationTokenSource linked =
                    CancellationTokenSource.CreateLinkedTokenSource(runCts.Token, destroyToken);
                CancellationToken token = linked.Token;

                try
                {
                    decorator.Run(DecoratorPhase.Start, ctx, NoCompleteOption, FinishCancel);
                    //시퀀스가 나눠야할 시간의 총길이를 가져옴
                    //총 길이는 선택한 _def를 resolver가 구해주는 방식으로 계산
                    float channelingSecond = await _lengthResolver.ResolveSeconds(ctx, _def.channelingLength, token);
                
                    if (channelingSecond <= 0)
                    {
                        FinishCancel();
                        Debug.Assert(false, "ChannelingSecond has not been allowed zero seconds");
                        return;
                    }
                    
                    float channelingElapsedTime = 0;
                    if ((ctx.Caster.CurrentStateType is CommonSkillState skillState) == true)
                    {
                        _animHash = skillState.CurrentAnimHash;
                    }

                    //채널링
                    while (finished == false && channelingElapsedTime < channelingSecond)
                    {
                        if (token.IsCancellationRequested)
                        {
                            FinishCancel();
                            return;
                        }

                        channelingElapsedTime += Time.deltaTime;
                        if (CheckInterruption())
                        {
                            FinishComplete(); //종료처리 성공으로 돌려서 쿨타임을 돌아가게 만들어야함.
                            return;
                        }

                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                    }

                    //채널링 이후 강제로 애니메이션 변경
                    //이걸 안하면 채널링 애니메이션이 루프타입이여서 안끝나서 어색할 수 있음.
                    PlayCasterIdleAnimation(playerBaseController);

                    //채널링 이후 애니메이션 실행로직
                    if (string.IsNullOrEmpty(_def.channelingAnimationNameAfterCasting) == true)
                    {
                        Debug.LogWarning($"There is no AnimationName {_def}");
                    }
                    else
                    {

                        float transitionduration = _def.channelingAnimTransitionDuration <= 0
                            ? 0.1f
                            : _def.channelingAnimTransitionDuration;
                        
                        playerModule.CommonSkillState.Prepare(
                            Animator.StringToHash(_def.channelingAnimationNameAfterCasting),
                            false, //애니메이션 락은 무조건 해제 락을 하면 루프에 무한으로 빠질 수 있음.
                            transitionduration
                        );
                        ctx.Caster.CurrentStateType = playerModule.CommonSkillState;
                    }

                    float hitSecond = await _lengthResolver.ResolveSeconds(ctx, _def.hitLength, token);
                    
                    if (hitSecond <= 0)
                    {
                        FinishCancel();
                        Debug.Assert(false, "HitLengthSecond has not been allowed zero seconds");
                        return;
                    }

                    
                    
                    
                    float hitElapsedTime = 0;
                    int nextHitIndex = 0;

                    HitEventDef[] hits = _def.hits;
                    if (hits == null || hits.Length == 0)
                    {
                        onComplete?.Invoke();
                        Debug.Assert(false, "hit is null");
                        return;
                    }

                    HitEventDef[] sorted = (HitEventDef[])hits.Clone();
                    Array.Sort(sorted, (x, y) => x.normalizedTime.CompareTo(y.normalizedTime));

                    while (finished == false && hitElapsedTime < hitSecond)
                    {
                        if (token.IsCancellationRequested)
                        {
                            FinishCancel();
                            return;
                        }

                        hitElapsedTime += Time.deltaTime;
                        float normalized = hitElapsedTime / hitSecond;

                        if (normalized < 0f) normalized = 0f;
                        if (normalized > 1f) normalized = 1f;


                        while (nextHitIndex < sorted.Length && normalized >= sorted[nextHitIndex].normalizedTime)
                        {
                            bool isLastHit = (nextHitIndex == sorted.Length - 1);
                            targeting.FillHitTargets(ctx);
                            
                            decorator.Run(DecoratorPhase.Tick, ctx, NoCompleteOption, NoCompleteOption);
                            if (isLastHit)
                            {
                                decorator.Run(DecoratorPhase.End, ctx, NoCompleteOption, NoCompleteOption);
                                effect.Apply(ctx, FinishComplete, FinishCancel);
                            }
                            else
                            {
                                effect.Apply(ctx, NoCompleteOption, FinishCancel);
                            }

                            nextHitIndex++;
                        }

                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    FinishCancel();
                }
                finally
                {
                    decorator.Release();
                    linked.Dispose();
                    runCts.Dispose();
                    if (ReferenceEquals(_cts, runCts))
                        _cts = null;
                }

                //complete권한이 없는 옵션
                void NoCompleteOption()
                {
                }

                void FinishCancel()
                {
                    if (finished) return;
                    finished = true;
                    onCancel?.Invoke();
                }

                void FinishComplete()
                {
                    Debug.Log("FinishComplete");

                    if (finished) return;
                    finished = true;
                    onComplete?.Invoke();
                }


                //현재 상태가 스킬에 의한 상태가 아닌 경우
                bool CheckInterruption()
                {
                    //현재 채널링에 쓰인 애니메이션이 아닌 다른 애니메이션일때
                    //같은 채널링 스킬을 쓰면 문제가 될 수있긴 하네
                    //같은 애니메이션을 쓸 때는 애니메이션 레이어에 
                    if ((_interrupt & InterruptFlag.OnAnimationMismatch) != 0)
                    {
                        if (ctx.Caster.CurrentStateType is CommonSkillState state)
                        {
                            if (state.CurrentAnimHash != _animHash)
                                return true;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    // 피격 체크 (Logic Data 체크)
                    if ((_interrupt & InterruptFlag.OnTakeDamage) != 0)
                    {
                        if (casterDamageable != null && casterDamageable.LastDamagedTime > currentTime)
                        {
                            //피격시 애니메이션 강제변경
                            PlayCasterIdleAnimation(playerBaseController);
                            return true;
                        }
                    }

                    return false;
                }

                void PlayCasterIdleAnimation(BaseController baseController)
                {
                    baseController.CurrentStateType = baseController.BaseIDleState;
                }
            }
        }
    }
}