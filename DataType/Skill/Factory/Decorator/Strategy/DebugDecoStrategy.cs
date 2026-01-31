using System;
using Controller;
using DataType.Skill.Factory.Decorator.Def;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory.Decorator.Strategy
{
    public class DebugDecoStrategy : IStackElementDecoratorStrategy
    {
        public Type DefType => typeof(DebugEndDecoDef);

        public IDecoratorModule Create(IDecoratorDef def, BaseController owner)
        {
            return new Module();
        }


        private sealed class Module : IDecoratorModule
        {
            public void Run(DecoratorPhase phase, SkillExecutionContext ctx, Action onComplete, Action onCancel)
            {
                Debug.Log("Debug End decorator");
                onComplete.Invoke();
            }

            public void Release()
            {
            }
        }
    }
}