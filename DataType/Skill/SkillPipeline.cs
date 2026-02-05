using System;
using DataType.Skill.Factory.Decorator;
using DataType.Skill.Factory.Effect;
using DataType.Skill.Factory.Sequence;
using DataType.Skill.Factory.Target;
using Skill;
using Zenject;

namespace DataType.Skill
{
    public sealed class SkillPipeline : ISkillPipeline
    {
        private readonly ITargetingModule _targeting;
        private readonly ISequenceModule _sequence;
        private readonly IDecoratorModule _decorator;
        private readonly IEffectModule _effect;

        //이거 Inject아님 생성자로 만들어지는거라 Inject를 하면 안됨.
        //뭐 해도 동작은 동일하지만.
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
            
            _targeting.BeginSelection(ctx,OnReady,CancelOnce);
            void OnReady()
            {
                if (ctx != null && ctx.IsCancelled) { CancelOnce(); return; }
                _sequence.Execute(ctx, _targeting, _decorator, _effect, CompleteOnce, CancelOnce);
            }
            
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

          
        }
    }
}