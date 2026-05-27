using System;
using Controller;
using Skill;

namespace DataType.Skill.Factory.Target
{
    public interface ITargetingStrategy
    {
        Type DefType { get; }
        ITargetingModule Create(ITargetingDef def, BaseController owner);
    }

    public interface ITargetingModule
    {
        void BeginSelection(SkillExecutionContext ctx, Action onComplete, Action onCancel);

        void FillHitTargets(SkillExecutionContext ctx);

        void Release();
    }

}