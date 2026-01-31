using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataType.Skill.Factory.Sequence.Def;
using DataType.Skill.Factory.Sequence.GetLength.Def;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory.Sequence.GetLength.Strategy
{
    public sealed class MeleeComboLengthCustomStrategy : IMeleeComboLengthStrategy
    {
        public Type DefType => typeof(LengthCustom);

        public UniTask<float> ResolveSeconds(SkillExecutionContext ctx, ISequenceLengthDef lengthDef,
            CancellationToken token)
        {
            if (lengthDef is LengthCustom def)
            {
                float second = def.seconds;
                return UniTask.FromResult(second);
            }
            Debug.Assert(false,"def is not IMeleeComboSequenceLengthDef");
            return UniTask.FromResult(0f);
        }
    }
}