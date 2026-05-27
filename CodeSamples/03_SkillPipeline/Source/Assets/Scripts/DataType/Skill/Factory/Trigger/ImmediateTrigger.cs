using System;
using DataType.Strategies;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory.Trigger
{
    public sealed class ImmediateTriggerStrategy : ISkillTriggerStrategy
    {
        public Type DefType => typeof(ImmediateTriggerDef);

        public void Fire(
            SkillExecutionContext ctx,
            ITriggerDef def,
            Action onCommit,
            Action onCancel)
        {
            
            onCommit?.Invoke();
        }
    }
}