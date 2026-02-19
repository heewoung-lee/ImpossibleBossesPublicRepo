using System;
using System.Threading;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Cysharp.Threading.Tasks;
using DataType.Skill.Factory.Sequence.Def;
using DataType.Skill.Factory.Sequence.GetLength.Def;
using DataType.Skill.ShareDataDef;
using Module.PlayerModule;
using Skill;
using UnityEngine;
using Util;

namespace DataType.Skill.Factory.Sequence.GetLength.Strategy
{
    public sealed class MeleeComboLengthAnimationStrategy : IMeleeComboLengthStrategy
    {

        public Type DefType => typeof(LengthAnimation);

        public async UniTask<float> ResolveSeconds(SkillExecutionContext ctx, ISequenceLengthDef lengthDef, CancellationToken token)
        {
            if (lengthDef is not LengthAnimation def)
            {
                Debug.Assert(false, "lengthDef is not MeleeComboLengthAnimation");
                return 0;
            }
                
            if (def.animNameRef == null) return 0.01f;

            AnimInfoDefStruct info = def.animNameRef.Resolve(ctx);
            string stateName = info.AnimationName;
            if (string.IsNullOrEmpty(stateName)) return 0.01f;
            

            int stateHash = Animator.StringToHash(stateName);
            ModulePlayerAnimInfo animInfo = ctx.Caster.GetComponent<ModulePlayerAnimInfo>();
            Debug.Assert(animInfo != null, $"{ctx.Caster.name} animInfo != null");

            AnimatorStateInfo st = await animInfo.GetStateInfo(stateHash, token);
            
            float seconds = st.length;

            return seconds;
        }

       
    }
}
