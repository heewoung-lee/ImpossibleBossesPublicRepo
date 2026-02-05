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
    public sealed class AreaVfxDecoratorStrategy : IStackElementDecoratorStrategy
    {
        public Type DefType => typeof(AreaVfxDecoratorDef);

        private readonly IVFXManagerServices _vfxManager;

        [Inject]
        public AreaVfxDecoratorStrategy(IVFXManagerServices vfxManager)
        {
            _vfxManager = vfxManager;
        }

        // 반환 타입: IDecoratorModule 로 통일
        public IDecoratorModule Create(IDecoratorDef def, BaseController owner)
        {
            var typed = def as AreaVfxDecoratorDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            return new Module(typed, owner, _vfxManager);
        }

        private sealed class Module : IDecoratorModule
        {
            private readonly AreaVfxDecoratorDef _def;
            private readonly BaseController _owner;
            private readonly IVFXManagerServices _vfxManager;

            public Module(AreaVfxDecoratorDef def, BaseController owner, IVFXManagerServices vfxManager)
            {
                _def = def;
                _owner = owner;
                _vfxManager = vfxManager;
            }

            public void Run(DecoratorPhase phase, SkillExecutionContext ctx, Action onComplete, Action onCancel)
            {
                string vfxPath = _def.vfxInfo.hitVfxPath.Resolve(ctx);
                
                if (string.IsNullOrWhiteSpace(vfxPath))
                {
                    onComplete?.Invoke();
                    return;
                }

                // 선택 지점이 없으면 스킵 (0,0,0 센티넬 금지)
                if (ctx == null || !ctx.SelectedPoint.HasValue)
                {
                    onComplete?.Invoke();
                    return;
                }

                float duration = 1f;
                if (_def.vfxInfo != null)
                    duration = _def.vfxInfo.hitVfxDuration.Resolve(ctx);

                Vector3 selectPoint = ctx.SelectedPoint.Value;

                float radius = _def.scaleRef.Resolve(ctx);
                _vfxManager.InstantiateParticleInArea(vfxPath, selectPoint, duration,localScale: Vector3.one * radius);

                onComplete?.Invoke();
            }

            public void Release()
            {
            }
        }
    }
}
