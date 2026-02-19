using System;
using System.Collections.Generic;
using Controller;
using DataType.Skill.Factory.Effect.Def;
using GameManagers.Interface.VFXManager;
using Skill;
using UnityEngine;
using Util;
using Zenject;

namespace DataType.Skill.Factory.Effect.Strategy
{
    public class ArcSpreadProjectileStrategy : IEffectStrategy
    {
        private readonly IVFXManagerServices _vfxManagerServices;

        [Inject]
        public ArcSpreadProjectileStrategy(IVFXManagerServices vfxManagerServices)
        {
            _vfxManagerServices = vfxManagerServices;
        }
        public Type DefType => typeof(ArcSpreadProjectileDef);

        public IEffectModule Create(IEffectDef def, BaseController owner)
        {
            var typed = def as ArcSpreadProjectileDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            return new Module(typed, _vfxManagerServices);
        }

        private sealed class Module : IEffectModule
        {
            private readonly ArcSpreadProjectileDef _arcSpreadProjectileDef;
            private readonly IVFXManagerServices _vfxManagerServices;

            public Module(ArcSpreadProjectileDef arcSpreadProjectileDef,IVFXManagerServices vfxManagerServices)
            {
                _arcSpreadProjectileDef = arcSpreadProjectileDef;
                _vfxManagerServices = vfxManagerServices;
            }

            public void Apply(ExecutionContext ctx, Action onComplete, Action onCancel)
            {
                if(String.IsNullOrEmpty(_arcSpreadProjectileDef.projectilePrefabPath) == true)
                    UtilDebug.LogError("projectilePath is null");
                
                
                List<Quaternion> shotRotations = TargetInSight.GenerateSpreadRotations(
                    ctx.Caster.transform,
                    _arcSpreadProjectileDef.spreadAngle,
                    _arcSpreadProjectileDef.projectileCount
                );
                
                foreach (Quaternion rot in shotRotations)
                {
                    _vfxManagerServices.InstantiateParticleWithTarget(
                        _arcSpreadProjectileDef.projectilePrefabPath,
                        ctx.Caster.transform, rot,
                        _arcSpreadProjectileDef.lifeTime);
                }
                onComplete?.Invoke();
            }
        }
    }
}