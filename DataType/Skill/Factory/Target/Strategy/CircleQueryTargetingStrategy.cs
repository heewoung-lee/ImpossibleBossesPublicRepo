using System;
using System.Collections.Generic;
using Controller;
using DataType.Skill.Factory.Target.Def;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory.Target.Strategy
{

    public sealed class CircleQueryTargetingStrategy : ITargetingStrategy
    {
        public Type DefType => typeof(CircleQueryTargetingDef);

        public ITargetingModule Create(ITargetingDef def, BaseController owner)
            => new Module(def);

        private sealed class Module : ITargetingModule
        {
            private ITargetingDef _def;
            private Collider[] _buffer = new Collider[64];

            public Module(ITargetingDef def)
            {
                _def = def;
            }

            public void BeginSelection(SkillExecutionContext ctx, Action onReady, Action onCancel)
            {
                onReady?.Invoke();
            }
            public void FillHitTargets(SkillExecutionContext ctx)
            {
                if (ctx == null)
                {
                    Debug.Assert(false,"ctx is null");
                    return;
                }

                ctx.HitTargets = Array.Empty<Collider>();

                if (_def is CircleQueryTargetingDef circleQueryDef == false)
                {
                    Debug.Assert(false,"CircleQueryTargetingDef is not Current Def");
                    return;
                }

                CircleQueryDef circleQuery = circleQueryDef.circleQueryDef;
                
                BaseController caster = ctx.Caster;
                if (caster == null)
                {
                    Debug.Assert(false,"caster is null");
                    return;
                }

                Transform casterTr = caster.transform;

                Vector3 origin = casterTr.position + casterTr.forward * circleQuery.forwardOffset;
                Vector3 p0 = origin + Vector3.up * 0.1f;
                Vector3 p1 = origin + Vector3.up * Mathf.Max(circleQuery.height, 0.2f);

                int count = Physics.OverlapCapsuleNonAlloc(
                    p0, p1, circleQuery.radius,
                    _buffer,
                    circleQuery.mask,
                    QueryTriggerInteraction.Collide);

                if (count <= 0)return;
                Collider[] result;
                if (circleQuery.angle >= 360f || circleQuery.angle <= 0f)
                    result = CopyFiltered(count, null, casterTr);
                else
                    result = CopyFiltered(count, circleQuery.angle * 0.5f, casterTr);

                if (result == null) return;
                if (result.Length == 0) return;

                ctx.HitTargets = result;
            }

            private Collider[] CopyFiltered(int n, float? halfAngle, Transform casterTr)
            {
                List<Collider> list = new List<Collider>(n);
                HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

                Vector3 fwd = casterTr.forward;
                fwd.y = 0f;
                if (fwd.sqrMagnitude > 0.0001f) fwd.Normalize();

                for (int i = 0; i < n; i++)
                {
                    Collider col = _buffer[i];
                    if (col == null) continue;

                    if (halfAngle.HasValue)
                    {
                        Vector3 dir = col.bounds.center - casterTr.position;
                        dir.y = 0f;
                        if (dir.sqrMagnitude < 0.0001f) continue;
                        dir.Normalize();

                        float ang = Vector3.Angle(fwd, dir);
                        if (ang > halfAngle.Value) continue;
                    }
                    
                    if (uniqueObjects.Contains(col.gameObject)) 
                        continue;
                    
                    uniqueObjects.Add(col.gameObject);
                    list.Add(col);
                }

                if (list.Count == 0) return Array.Empty<Collider>();
                return list.ToArray();
            }

            public void Release()
            {
                // 선택 상태가 없으니 정리할 것도 거의 없음
            }
        }
    }
}