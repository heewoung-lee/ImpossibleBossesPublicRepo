using System;
using Controller;
using Controller.CrowdControl;
using DataType.Skill.Factory.Effect.Def;
using Skill;
using UnityEngine;
using Util;

namespace DataType.Skill.Factory.Effect.Strategy
{
    public class CrowdControlStrategy : IEffectStrategy
    {
        public Type DefType => typeof(CrowdControlDef);

        public IEffectModule Create(IEffectDef def, BaseController owner)
        {
            return new Module((CrowdControlDef)def, owner);
        }

        private sealed class Module : IEffectModule
        {
            private readonly CrowdControlDef _def;
            private readonly BaseController _caster;

            public Module(CrowdControlDef def, BaseController owner)
            {
                _def = def;
                _caster = owner;
            }

            public void Apply(ExecutionContext ctx, Action onComplete, Action onCancel)
            {
                if (ctx == null)
                {
                    onCancel?.Invoke();
                    return;
                }

                if (ctx is SkillExecutionContext skillContext == false)
                {
                    Debug.Assert(false, "check ctx it is not skillCtx");
                    return;
                }

                Collider[] targets = skillContext.HitTargets;
                foreach (Collider target in targets)
                {
                    if (target.TryGetComponent(out BaseController enemyController))
                    {
                        ApplyCC(_caster, enemyController, _def, _def.hitVfxDuration.Resolve(skillContext));
                    }
                }

                onComplete?.Invoke();
            }

            private void ApplyCC(BaseController caster, BaseController enemy, CrowdControlDef def, float duration)
            {
                switch (def.ccType)
                {
                    case CCType.Taunt:
                        if (enemy.TryGetComponent(out ICCReceiver ccReceiver))
                        {
                            ccReceiver.ApplyCC(def.ccType, caster.gameObject, duration);
                        }

                        break;
                }
            }
        }
    }
}
