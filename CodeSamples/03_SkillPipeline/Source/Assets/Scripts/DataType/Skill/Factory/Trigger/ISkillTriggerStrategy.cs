using System;
using Character.Skill;
using Skill;

namespace DataType.Skill.Factory.Trigger
{
    public interface ISkillTriggerStrategy
    {
        Type DefType { get; }
        void Fire(SkillExecutionContext ctx, ITriggerDef def, Action onCommit, Action onCancel);
    }

}