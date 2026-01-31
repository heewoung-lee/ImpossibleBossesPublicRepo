using System;
using DataType.Skill.Factory.Decorator;
using DataType.Skill.Factory.Effect;
using DataType.Skill.Factory.Target;
using Skill;

namespace DataType.Skill.Factory.Sequence
{
    public interface ISequenceModule
    {
        void Execute(
            SkillExecutionContext ctx,
            ITargetingModule targeting,
            IDecoratorModule decorator,
            IEffectModule effect,
            Action onComplete,
            Action onCancel);

        void Release();
    }
}