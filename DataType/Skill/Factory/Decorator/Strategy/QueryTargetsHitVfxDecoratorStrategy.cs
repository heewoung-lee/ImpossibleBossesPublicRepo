using System;
using Controller;
using DataType.Skill.Factory.Decorator.Def;
using GameManagers.Interface.VFXManager;
using Skill;
using UnityEngine;
using Util;
using Zenject;

namespace DataType.Skill.Factory.Decorator.Strategy
{
    public sealed class QueryTargetsHitVfxDecoratorStrategy : IStackElementDecoratorStrategy
    {
        public Type DefType => typeof(QueryTargetsDecoratorDef);

        private readonly IVFXManagerServices _vfxManager;

        [Inject]
        public QueryTargetsHitVfxDecoratorStrategy(IVFXManagerServices vfxManager)
        {
            _vfxManager = vfxManager;
        }

        public IDecoratorModule Create(IDecoratorDef def, BaseController owner)
        {
            var typed = def as QueryTargetsDecoratorDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            return new Module(typed, _vfxManager);
        }

        private sealed class Module : IDecoratorModule
        {
            private readonly QueryTargetsDecoratorDef _def;
            private readonly IVFXManagerServices _vfxManager;

            public Module(QueryTargetsDecoratorDef def, IVFXManagerServices vfxManager)
            {
                _def = def;
                _vfxManager = vfxManager;
            }

            public void Run(DecoratorPhase phase, SkillExecutionContext ctx, Action onComplete, Action onCancel)
            {

                if (ctx == null)
                {
                    
                    if (onComplete != null) onComplete();
                    return;
                }

                Collider[] cols = ctx.HitTargets;
                if (cols == null || cols.Length == 0)
                {
                    if (onComplete != null) onComplete();
                    return;
                }

                
                SpawnHitVfx(cols, ctx);
                if (onComplete != null) onComplete();
            }

            public void Release()
            {
                
            }

            private void SpawnHitVfx(Collider[] cols, SkillExecutionContext ctx)
            {
                if (_vfxManager == null) return;
                if (_def == null) return;
                if (_def.hitVfx == null) return;

                string vfxPath = _def.hitVfx.hitVfxPath.Resolve(ctx);
                if (string.IsNullOrWhiteSpace(vfxPath)) return;

                for (int i = 0; i < cols.Length; i++)
                {
                    Collider col = cols[i];
                    if (col == null) continue;

                    Transform targetTransform = col.transform;

                    if (_def.hitVfx.checkHitVfxLocatedHead)
                    {
                        HeadTr head = targetTransform.GetComponentInChildren<HeadTr>();
                        if (head != null) targetTransform = head.transform;
                    }

                    float duration = 1f;
                    if (_def.hitVfx.hitVfxDuration != null)
                        duration = _def.hitVfx.hitVfxDuration.Resolve(ctx);

                    _vfxManager.InstantiateParticleWithTarget(vfxPath, targetTransform, duration, true);
                }
            }
        }
    }
}
