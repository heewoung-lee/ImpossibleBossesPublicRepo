using System;
using DataType.Skill.Factory.Decorator;
using DataType.Skill.Factory.Effect;
using DataType.Skill.Factory.Sequence;
using DataType.Skill.Factory.Target;
using Skill;
using UnityEngine;

namespace DataType.Skill
{
    public sealed class SkillPipeline : ISkillPipeline
    {
        private readonly ITargetingModule _targeting;
        private readonly ISequenceModule _sequence;
        private readonly IDecoratorModule _decorator;
        private readonly IEffectModule _effect;

        public SkillPipeline(
            ITargetingModule targeting,
            ISequenceModule sequence,
            IDecoratorModule decorator,
            IEffectModule effect)
        {
            _targeting = targeting;
            _sequence = sequence;
            _decorator = decorator;
            _effect = effect;
        }

        public void Execute(SkillExecutionContext ctx, Action onComplete, Action onCancel)
        {
            bool finished = false;

            void CleanupOnce()
            {
                if (_sequence != null) _sequence.Release();
                if (_targeting != null) _targeting.Release();
            }

            void CompleteOnce()
            {
                if (finished) return;
                finished = true;
                CleanupOnce();
                onComplete?.Invoke();
            }

            void CancelOnce()
            {
                if (finished) return;
                finished = true;
                CleanupOnce();
                onCancel?.Invoke();
            }

            _targeting.BeginSelection(
                ctx,
                onReady: () =>
                {
                    if (ctx != null && ctx.IsCancelled) { CancelOnce(); return; }
                    _sequence.Execute(ctx, _targeting, _decorator, _effect, CompleteOnce, CancelOnce);
                },
                onCancel: CancelOnce);
        }
    }
}