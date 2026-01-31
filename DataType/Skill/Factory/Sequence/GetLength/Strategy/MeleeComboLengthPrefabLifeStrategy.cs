using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataType.Skill.Factory.Sequence.Def;
using DataType.Skill.Factory.Sequence.GetLength.Def;
using GameManagers.Interface.VFXManager;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory.Sequence.GetLength.Strategy
{
    public sealed class MeleeComboLengthPrefabLifeStrategy : IMeleeComboLengthStrategy
    {
        public Type DefType => typeof(LengthPrefabLife);

        private readonly IVFXManagerServices _vfx;

        public MeleeComboLengthPrefabLifeStrategy(IVFXManagerServices vfx)
        {
            _vfx = vfx;
        }

        public UniTask<float> ResolveSeconds(SkillExecutionContext ctx, ISequenceLengthDef lengthDef, CancellationToken token)
        {

            if (lengthDef is LengthPrefabLife def)
            {
                if (_vfx == null)
                {
                    Debug.Assert(false,"VFX Manager is Null");
                    return UniTask.FromResult(0f);
                }

                string vfxPrefabPath = def.vfxPathDef.Resolve(ctx);
                
                if (string.IsNullOrEmpty(vfxPrefabPath))
                {
                    Debug.Assert(false,$"PrefabPath is not found Prefab{vfxPrefabPath}");
                    return UniTask.FromResult(0f);
                }
                float life = _vfx.GetParticleLifeCycle(vfxPrefabPath);
                return UniTask.FromResult(life);
            }
            Debug.Assert(false,"def is not IMeleeComboSequenceLengthDef");
            return UniTask.FromResult(0f);
        }
    }
}