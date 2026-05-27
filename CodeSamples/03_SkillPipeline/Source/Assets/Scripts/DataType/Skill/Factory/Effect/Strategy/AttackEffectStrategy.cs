using System;
using Controller;
using DataType.Skill.Factory.Effect.Def;
using DataType.Skill.ShareDataDef;
using Skill;
using Stats.BaseStats;
using UnityEngine;

namespace DataType.Skill.Factory.Effect.Strategy
{
    public sealed class AttackEffectStrategy : IEffectStrategy
    {
        public Type DefType => typeof(AttackEffectDef);

        public IEffectModule Create(IEffectDef def, BaseController owner)
            => new Module((AttackEffectDef)def, owner);

        private sealed class Module : IEffectModule
        {
            private readonly AttackEffectDef _def;

            public Module(AttackEffectDef def, BaseController owner)
            {
                _def = def;
            }

            public void Apply(ExecutionContext ctx, Action onComplete, Action onCancel)
            {
                BaseStats stats = ctx.Caster.GetComponent<BaseStats>();
                if (ctx is SkillExecutionContext context)
                {
                    Debug.Assert(_def != null, "_def is null");

                    float mul = _def.multiplier;
                    float add = _def.additional;

                    int damage = Mathf.RoundToInt(stats.Attack * mul + add);

                   

                    if (ctx.Caster.TryGetComponent(out IAttackRange attacker))
                    {
                        foreach (Collider target in context.HitTargets)
                        {
                            // if(target is null) continue;
                            // //상황에 따라 hitTagets이 null이 될 수 있음
                            // //예를 들어 체인 라이트닝의 경우 다음 후보가 없으면 targeting이 null이 됨 그래서
                            // //null Target팅 방어용
                            
                            if (target.TryGetComponent(out IDamageable targetDamageable))
                            {
                                
                                //특수조건 처리
                                int finalDamage = damage;
                                if (_def.specialAttackCondition != null)
                                {
                                    finalDamage = _def.specialAttackCondition.CalculateSpecialDamage(finalDamage, ctx.Caster.transform,target.transform);
                                }
                                
                                targetDamageable.OnAttacked(attacker, finalDamage);
                            }
                        }
                    }
                }
                else
                    Debug.Assert(false, "ctx is not SkillExecutionContext");

                onComplete?.Invoke();
            }
        }
    }
}