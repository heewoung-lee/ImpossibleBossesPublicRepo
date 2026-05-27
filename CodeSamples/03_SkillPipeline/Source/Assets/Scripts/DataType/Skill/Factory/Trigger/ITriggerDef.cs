using System;

namespace DataType.Skill.Factory.Trigger
{
    public interface ITriggerDef { }

    [Serializable]
    public sealed class ImmediateTriggerDef : ITriggerDef
    {
        // Immediate여서 필드가 없음
    }
}