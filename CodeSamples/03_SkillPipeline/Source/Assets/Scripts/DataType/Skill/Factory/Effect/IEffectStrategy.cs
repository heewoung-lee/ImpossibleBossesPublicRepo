using System;
using Controller;
using Skill;

namespace DataType.Skill.Factory.Effect
{
    public interface IEffectStrategy
    {
        Type DefType { get; }
        IEffectModule Create(IEffectDef def, BaseController owner);
    }

    public interface IEffectModule
    {
        void Apply(ExecutionContext ctx, Action onComplete, Action onCancel);
    }
}