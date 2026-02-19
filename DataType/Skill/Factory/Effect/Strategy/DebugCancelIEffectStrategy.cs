using System;
using Controller;
using UnityEngine;
using Util;

namespace DataType.Skill.Factory.Effect
{
    [Serializable]
    public sealed class DebugCancelEffectDef : IEffectDef
    {
        public string message = "DebugCancelEffect";
        public float Value { get; }
    }

    public sealed class DebugCancelEffectStrategy : IEffectStrategy
    {
        public Type DefType => typeof(DebugCancelEffectDef);

        public IEffectModule Create(IEffectDef def, BaseController owner)
            => new Module((DebugCancelEffectDef)def, owner);

        private sealed class Module : IEffectModule
        {
            private readonly DebugCancelEffectDef _def;
            private readonly BaseController _owner;

            public Module(DebugCancelEffectDef def, BaseController owner)
            {
                _def = def;
                _owner = owner;
            }

            public void Apply(ExecutionContext ctx, Action onComplete, Action onCancel)
            {
                UtilDebug.Log($"[DebugCancelEffect] Cancel Fired / Caster={_owner.name} / Msg={_def.message}");
                onCancel?.Invoke(); // 핵심: Complete가 아니라 Cancel 호출
            }
        }
    }
}