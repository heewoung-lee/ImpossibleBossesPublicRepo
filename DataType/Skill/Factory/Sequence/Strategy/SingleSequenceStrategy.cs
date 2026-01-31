using System;
using Controller;
using DataType.Skill.Factory.Decorator;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Effect;
using DataType.Skill.Factory.Sequence.Def;
using DataType.Skill.Factory.Target;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory.Sequence.Strategy
{
    public sealed class SingleSequenceStrategy : ISequenceStrategy
    {
        public Type DefType => typeof(SingleSequenceDef);

        public ISequenceModule Create(ISequenceDef def, BaseController owner)
        {
            return new Module();
        }

        private sealed class Module : ISequenceModule
        {
            public void Execute(
                SkillExecutionContext ctx,
                ITargetingModule targeting,
                IDecoratorModule decorator,
                IEffectModule effect,
                Action onComplete,
                Action onCancel)
            {
                
                
                if (ctx == null) { onCancel?.Invoke(); return; }
                if (targeting == null) { onCancel?.Invoke(); return; }
                if (decorator == null) { onCancel?.Invoke(); return; }
                if (effect == null) { onCancel?.Invoke(); return; }

                bool finished = false;

                // “헛스윙 허용”이면 ok 실패로 취소하지 말 것
                targeting.FillHitTargets(ctx);
                // 이 모듈은 시작-> 데코-> 이펙트 순으로 진행
                decorator.Run(DecoratorPhase.Start, ctx,OnCompleteStartDeco,CancelOnce);
                void OnCompleteStartDeco()
                {
                    if (finished) return;
                    OnTickDecoRun();
                }
                void OnTickDecoRun()
                {
                    if (finished) return;
                    decorator.Run(DecoratorPhase.Tick, ctx, OnCompleteTickDeco, CancelOnce);
                }
                void OnCompleteTickDeco()
                {
                    if (finished) return;
                    OnEndDecoRun();
                }
                void OnEndDecoRun()
                {
                    decorator.Run(DecoratorPhase.End, ctx, OnCompleteEndDeco, CancelOnce);
                }
                void OnCompleteEndDeco()
                {
                    if (finished) return;
                    RunEffect();
                }
                void RunEffect()
                {
                    if (finished) return;
                    effect.Apply(ctx, CompleteOnce, CancelOnce);
                }
                void CompleteOnce()
                {
                    if (finished) return; 
                    finished = true; 
                    onComplete?.Invoke();
                }
                void CancelOnce()
                {
                    if (finished) return; 
                    finished = true; 
                    onCancel?.Invoke();
                }
            }
            public void Release()
            {
                // SingleSequence는 취소할 async 작업이 없음 -> 아무 것도 하지 않는다
            }
        }
    }
}