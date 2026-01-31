using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataType.Skill.Factory.Sequence.Def;
using DataType.Skill.Factory.Sequence.GetLength.Def;
using DataType.Skill.ShareDataDef;
using Skill;
using UnityEngine;

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

            Animator animator = null;
            if (ctx != null && ctx.Caster != null)
                animator = ctx.Caster.GetComponentInChildren<Animator>();

            if (animator == null) return 0.01f;

            int stateHash = Animator.StringToHash(stateName);

            await WaitUntilState(animator, stateHash, token);

            AnimatorStateInfo st;
            if (animator.IsInTransition(0))
                st = animator.GetNextAnimatorStateInfo(0);
            else
                st = animator.GetCurrentAnimatorStateInfo(0);

            float seconds = st.length;


            return seconds;
        }

        private async UniTask WaitUntilState(Animator animator, int stateHash, CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.shortNameHash == stateHash || info.fullPathHash == stateHash)
                    return;

                if (animator.IsInTransition(0))
                {
                    AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(0);
                    if (next.shortNameHash == stateHash || next.fullPathHash == stateHash)
                        return;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
    }
}
