using System;
using System.Collections.Generic;
using Controller;
using DataType.Skill.Factory.Effect.Def;
using GameManagers.Interface.VFXManager;
using NetWork;
using Skill;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace DataType.Skill.Factory.Effect.Strategy
{
    public class GenerateAreaProjectileStrategy : IEffectStrategy
    {
        private readonly IVFXManagerServices _vfxManagerServices;

        [Inject]
        public GenerateAreaProjectileStrategy(IVFXManagerServices vfxManagerServices)
        {
            _vfxManagerServices = vfxManagerServices;
        }

        public Type DefType => typeof(GenerateProjectileDef);

        public IEffectModule Create(IEffectDef def, BaseController owner)
        {
            var typedef = def as GenerateProjectileDef;
            if (typedef == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            return new Module(typedef, _vfxManagerServices);
        }

        private sealed class Module : IEffectModule
        {
            private readonly GenerateProjectileDef _generateProjectileDef;
            private readonly IVFXManagerServices _vfxManagerServices;

            public Module(GenerateProjectileDef generateProjectileDef, IVFXManagerServices vfxManagerServices)
            {
                _generateProjectileDef = generateProjectileDef;
                _vfxManagerServices = vfxManagerServices;
            }

            public void Apply(ExecutionContext ctx, Action onComplete, Action onCancel)
            {
                if(ctx is null)
                    Debug.Assert(false, $"ctx is null");
                
                
                SkillExecutionContext skillExecutionContext = ctx as SkillExecutionContext;
                Vector3 selectArea = Vector3.zero;
                if (skillExecutionContext != null && skillExecutionContext.SelectedPoint != null)
                {
                    selectArea = skillExecutionContext.SelectedPoint.Value;
                }

                if (selectArea == Vector3.zero) Debug.Log("Select area is null");

                if (String.IsNullOrEmpty(_generateProjectileDef.projectilePrefabPath) == true)
                    Debug.LogError("projectilePath is null");


                NetworkParams networkParams = new NetworkParams
                    (
                        argFloat : _generateProjectileDef.floatParams,
                        argString:  _generateProjectileDef.stringParams,
                        argInteger:  _generateProjectileDef.integerParams,
                        argUlong : ctx.Caster.GetComponent<NetworkObject>().NetworkObjectId,
                        argBoolean:  _generateProjectileDef.booleanParams
                        ){};
                
                _vfxManagerServices.InstantiateParticleInArea(
                    _generateProjectileDef.projectilePrefabPath,
                    selectArea,
                    _generateProjectileDef.lifeTime,
                    networkParams: networkParams
                    );

                onComplete?.Invoke();
            }
        }
    }
}