using System;
using Controller;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Decorator.Def.Start;
using GameManagers.Interface.VFXManager;
using Skill;
using UnityEngine;
using Zenject;

namespace DataType.Skill.Factory.Decorator.Strategy
{
    // IStackElementDecoratorStrategy는 "마커"이고,
    // 최종적으로는 IDecoratorStrategy를 상속(또는 동일 시그니처)해야 함
    public sealed class CastVfxDecoratorStrategy : IStackElementDecoratorStrategy
    {
        public Type DefType => typeof(CastVfxDecoratorDef);

        private readonly IVFXManagerServices _vfxManager;

        [Inject]
        public CastVfxDecoratorStrategy(IVFXManagerServices vfxManager)
        {
            _vfxManager = vfxManager;
        }

        // 반환 타입: IDecoratorModule 로 통일
        public IDecoratorModule Create(IDecoratorDef def, BaseController owner)
        {
            var typed = def as CastVfxDecoratorDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            return new Module(typed, owner, _vfxManager);
        }

        private sealed class Module : IDecoratorModule
        {
            private readonly CastVfxDecoratorDef _def;
            private readonly BaseController _owner;
            private readonly IVFXManagerServices _vfxManager;

            public Module(CastVfxDecoratorDef def, BaseController owner, IVFXManagerServices vfxManager)
            {
                _def = def;
                _owner = owner;
                _vfxManager = vfxManager;
            }

            public void Run(DecoratorPhase phase, SkillExecutionContext ctx, Action onComplete, Action onCancel)
            {
                if (_def == null)
                {
                    if (onComplete != null) onComplete();
                    return;
                }

                string vfxPath = _def.castVfxPath.Resolve(ctx);
                if (string.IsNullOrWhiteSpace(vfxPath))
                {
                    if (onComplete != null) onComplete();
                    return;
                }

                Transform casterTr = _owner.transform;
                if (ctx != null && ctx.Caster != null)
                    casterTr = ctx.Caster.transform;

                float duration = 1f;

                // castVfxDuration 타입이 "Resolve(ctx)" 가능한 레퍼런스라고 가정
                // null이면 기본값 유지
                if (_def.castVfxDuration != null)
                    duration = _def.castVfxDuration.Resolve(ctx);

                _vfxManager.InstantiateParticleWithTarget(vfxPath, casterTr, duration);

                if (onComplete != null) onComplete();
            }

            public void Release()
            {
            }
        }
    }
}
