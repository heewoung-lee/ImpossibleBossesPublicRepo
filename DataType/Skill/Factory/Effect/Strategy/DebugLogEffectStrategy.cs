using System;
using Controller;
using Skill;
using UnityEngine;
using Util;

namespace DataType.Skill.Factory.Effect
{
    [Serializable]
    public sealed class DebugLogEffectDef : IEffectDef
    {
        public string message = "DebugEffect Fired";
        public float Value { get; }
    }

    public sealed class DebugLogEffectStrategy : IEffectStrategy
    {
        public Type DefType => typeof(DebugLogEffectDef);

        public IEffectModule Create(IEffectDef def, BaseController owner)
            => new Module((DebugLogEffectDef)def);

        private sealed class Module : IEffectModule
        {
            private readonly DebugLogEffectDef _def;

            public Module(DebugLogEffectDef def) => _def = def;

            public void Apply(ExecutionContext ctx, Action onComplete, Action onCancel)
            {
                if (ctx is SkillExecutionContext context)
                {
                    UtilDebug.Log($"<color=red>[DebugEffect]</color> {_def.message} / Caster={ctx.Caster.name}");

                    UtilDebug.Log($"[Effect] HitTargets Count = {context.HitTargets.Length}");
                    onComplete?.Invoke();
                }
            }
        }
    }
}