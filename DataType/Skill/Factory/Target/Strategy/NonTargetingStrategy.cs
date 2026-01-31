using System;
using Controller;
using DataType.Skill.Factory.Target.Def;
using Skill;

namespace DataType.Skill.Factory.Target.Strategy
{
    /// <summary>
    /// 타겟이 없는 스킬전용
    /// </summary>
    public class NonTargetingStrategy:ITargetingStrategy
    {
        public Type DefType  => typeof(NoneTargetingDef);
        public ITargetingModule Create(ITargetingDef def, BaseController owner)
        {
            return new Module();
        }
        
        private sealed class Module : ITargetingModule
        {
            public void BeginSelection(SkillExecutionContext ctx, Action onReady, Action onCancel)
            {
                onReady?.Invoke();
            }

            public void FillHitTargets(SkillExecutionContext ctx)
            {
            }

            public void Release()
            {
            }
        }
    }
}