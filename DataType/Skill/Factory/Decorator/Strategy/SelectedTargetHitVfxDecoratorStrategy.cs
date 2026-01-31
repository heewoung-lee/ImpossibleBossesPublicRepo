using System;
using Controller;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Decorator.Def.Tick;
using GameManagers.Interface.VFXManager;
using Skill;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace DataType.Skill.Factory.Decorator.Strategy
{
    public sealed class SelectedTargetHitVfxDecoratorStrategy : IStackElementDecoratorStrategy
    {
        public Type DefType => typeof(SelectedTargetHitVfxDecoratorDef);

        private readonly IVFXManagerServices _vfxManager;

        [Inject]
        public SelectedTargetHitVfxDecoratorStrategy(IVFXManagerServices vfxManager)
        {
            _vfxManager = vfxManager;
        }

        public IDecoratorModule Create(IDecoratorDef def, BaseController owner)
        {
            var typed = def as SelectedTargetHitVfxDecoratorDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            return new Module(typed, _vfxManager);
        }

        private sealed class Module : IDecoratorModule
        {
            private readonly SelectedTargetHitVfxDecoratorDef _def;
            private readonly IVFXManagerServices _vfxManager;

            public Module(SelectedTargetHitVfxDecoratorDef def, IVFXManagerServices vfxManager)
            {
                _def = def;
                _vfxManager = vfxManager;
            }

            public void Run(DecoratorPhase phase, SkillExecutionContext ctx, Action onComplete, Action onCancel)
            {
                
                Assert.IsNotNull(ctx,"[SelectedTargetHitVfxDecorator] Run() got null ctx. Pipeline/Decorator call is wrong.");
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
                Assert.IsNotNull(_vfxManager,
                    "[SelectedTargetHitVfxDecorator] _vfxManager is null. Check Zenject binding for IVFXManagerServices.");
                Assert.IsNotNull(_def,
                    "[SelectedTargetHitVfxDecorator] _def is null. Strategy.Create() should never pass null def.");
                Assert.IsNotNull(_def != null ? _def.hitVfx : null,
                    "[SelectedTargetHitVfxDecorator] _def.hitVfx is null. Check SelectedTargetHitVfxDecoratorDef.hitVfx assignment.");

                if (_vfxManager == null) return;
                if (_def == null) return;
                if (_def.hitVfx == null) return;

                string vfxPath = _def.hitVfx.hitVfxPath.Resolve(ctx);
                
                
                Assert.IsFalse(string.IsNullOrWhiteSpace(vfxPath),
                    "[SelectedTargetHitVfxDecorator] hitVfxPath is empty/whitespace. Check SelectedTargetHitVfxDecoratorDef.hitVfx.hitVfxPath.");

                if (string.IsNullOrWhiteSpace(vfxPath)) return;

                for (int i = 0; i < cols.Length; i++)
                {
                    Collider col = cols[i];
                    if (col == null) continue;

                    Transform tr = col.transform;
                    if (tr == null) continue;

                    float duration = 1f;
                    if (_def.hitVfx.hitVfxDuration != null)
                        duration = _def.hitVfx.hitVfxDuration.Resolve(ctx);
                    
                    _vfxManager.InstantiateParticleWithTarget(vfxPath, tr, duration, true);
                }
            }
        }
    }
}
