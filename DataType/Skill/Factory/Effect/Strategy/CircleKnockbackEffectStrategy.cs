using System;
using Controller;
using DataType.Skill.Factory.Effect.Def;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory.Effect.Strategy
{
    public sealed class CircleKnockbackEffectStrategy : IEffectStrategy
    {
        public Type DefType => typeof(CircleKnockbackDef);

        public IEffectModule Create(IEffectDef def, BaseController owner)
        {
            var typed = def as CircleKnockbackDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            return new Module(typed, owner);
        }

        private sealed class Module : IEffectModule
        {
            private readonly CircleKnockbackDef _def;
            private readonly BaseController _owner;

            public Module(CircleKnockbackDef def, BaseController owner)
            {
                _def = def;
                _owner = owner;
            }

            public void Apply(ExecutionContext ctx, Action onComplete, Action onCancel)
            {
                if (ctx == null)
                {
                    if (onCancel != null) onCancel.Invoke();
                    return;
                }

                if (ctx is SkillExecutionContext skillContext == false)
                {
                    Debug.Assert(false, "check ctx it is not skillCtx");
                    return;
                }


                Vector3 centerPos = _owner.transform.position;

                Collider[] colliders = skillContext.HitTargets;

                foreach (var col in colliders)
                {

                    if (col.TryGetComponent(out EnemyCrowdControlNetworkReceiver receiver))
                    {
                        Vector3 enemyPos = receiver.transform.position;
                        Vector3 dir = enemyPos - centerPos;
                        dir.y = 0;

                        float currentDist = dir.magnitude;
                        Vector3 pushDir = (currentDist < 0.1f) ? receiver.transform.forward : dir.normalized;

                        float pushDistance = _def.radius - currentDist;

                        if (pushDistance > 0)
                        {
                            receiver.EnemyPushBackRpc(pushDir, pushDistance, _def.pushDuration);
                        }
                    }
                }

                onComplete?.Invoke();
            }
        }
    }
}