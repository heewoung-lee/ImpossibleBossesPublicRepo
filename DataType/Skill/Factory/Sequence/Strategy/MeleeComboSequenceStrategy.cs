using System;
using System.Threading;
using Controller;
using Cysharp.Threading.Tasks;
using DataType.Skill.Factory.Decorator;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Effect;
using DataType.Skill.Factory.Sequence.Def;
using DataType.Skill.Factory.Sequence.GetLength.Strategy;
using DataType.Skill.Factory.Target;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory.Sequence.Strategy
{
    public sealed class MeleeComboSequenceStrategy : ISequenceStrategy
    {
        public Type DefType => typeof(MeleeComboSequenceDef);

        private readonly IMeleeComboLengthResolver _lengthResolver;

        public MeleeComboSequenceStrategy(IMeleeComboLengthResolver lengthResolver)
        {
            _lengthResolver = lengthResolver;
        }

        public ISequenceModule Create(ISequenceDef def, BaseController owner)
            => new Module((MeleeComboSequenceDef)def, _lengthResolver);


        private sealed class Module : ISequenceModule
        {
            private readonly MeleeComboSequenceDef _def;
            private readonly IMeleeComboLengthResolver _lengthResolver;
            private CancellationTokenSource _cts;

            public Module(MeleeComboSequenceDef def, IMeleeComboLengthResolver lengthResolver)
            {
                _def = def;
                _lengthResolver = lengthResolver;
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

                HitEventDef[] hits = _def.hits;
                if (hits == null || hits.Length == 0)
                {
                    onComplete?.Invoke();
                    Debug.Assert(false, "hit is null");
                    return;
                }


                HitEventDef[] sorted = (HitEventDef[])hits.Clone();
                Array.Sort(sorted, (x, y) => x.normalizedTime.CompareTo(y.normalizedTime));
                //SO원본데이터가 바뀔 수 있으므로 안전하게 클론떠서 정렬하고 클론으로 판단.


                bool finished = false;

                CancellationToken destroyToken = CancellationToken.None;
                if (ctx.Caster != null)
                    destroyToken = ctx.Caster.GetCancellationTokenOnDestroy();

                //ctx토큰과 캐스터의 토큰 둘중 하나만 사라져도 멈추게 작동
                CancellationTokenSource linked =
                    CancellationTokenSource.CreateLinkedTokenSource(runCts.Token, destroyToken);
                CancellationToken token = linked.Token;


                try
                {
                    // Start (1회)
                    decorator.Run(DecoratorPhase.Start, ctx, NoCompleteOption, FinishCancel);

                    //시퀀스가 나눠야할 시간의 총길이를 가져옴
                    //총 길이는 선택한 _def를 resolver가 구해주는 방식으로 계산


                    float totalSeconds = await _lengthResolver.ResolveSeconds(ctx, _def.length, token);
                    if (totalSeconds <= 0.0001f || float.IsNaN(totalSeconds) || float.IsInfinity(totalSeconds))
                    {
                        FinishCancel();
                        Debug.Assert(false, "length is not be read");
                        return;
                    }

                    int hitIndex = 0;
                    float elapsed = 0;

                    while (finished == false && hitIndex < sorted.Length)
                    {
                        if (token.IsCancellationRequested)
                        {
                            FinishCancel();
                            return;
                        }

                        elapsed += Time.deltaTime;
                        float normalized = elapsed / totalSeconds;

                        if (normalized < 0f) normalized = 0f;
                        if (normalized > 1f) normalized = 1f;


                        while (finished == false && hitIndex < sorted.Length &&
                               normalized >= sorted[hitIndex].normalizedTime)
                        {
                            bool isLast = (hitIndex == sorted.Length - 1);

                            // Targeting만 HitTargets 갱신
                            targeting.FillHitTargets(ctx);

                            // Tick (hit마다) - "연출"은 게임플레이 타이밍을 막으면 안 되므로 기다리지 않는다
                            decorator.Run(DecoratorPhase.Tick, ctx, NoCompleteOption, NoCompleteOption);

                            // Effect 적용
                            if (isLast)
                            {
                                decorator.Run(DecoratorPhase.End, ctx, NoCompleteOption, NoCompleteOption);
                                effect.Apply(ctx, FinishComplete, FinishCancel);
                            }
                            else
                            {
                                effect.Apply(ctx, NoCompleteOption, FinishCancel);
                            }

                            hitIndex++;
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
            }
        }
    }
}