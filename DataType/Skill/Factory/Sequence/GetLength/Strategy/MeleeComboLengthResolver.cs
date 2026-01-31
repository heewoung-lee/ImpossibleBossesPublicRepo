using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataType.Skill.Factory.Sequence.Def;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory.Sequence.GetLength.Strategy
{
    public interface IMeleeComboLengthStrategy
    {
        Type DefType { get; }
        UniTask<float> ResolveSeconds(SkillExecutionContext ctx, ISequenceLengthDef lengthDef, CancellationToken token);
    }

    public interface IMeleeComboLengthResolver
    {
        UniTask<float> ResolveSeconds(SkillExecutionContext ctx, ISequenceLengthDef lengthDef, CancellationToken token);
    }

    /// <summary>
    /// 밀리콤보시퀀스가 구간을 나눌때 필요한 총시간을 구해다 주는 클래스
    /// Def옵션으로 어떤 객체의 총시간을 구할지를 정하고 각 def의 전략에 IMeleeComboLengthStrategy를 상속해
    /// 총시간을 구해오도록 설계
    /// </summary>
    public sealed class MeleeComboLengthResolver : IMeleeComboLengthResolver
    {
        private readonly Dictionary<Type, IMeleeComboLengthStrategy> _map;

        public MeleeComboLengthResolver(List<IMeleeComboLengthStrategy> strategies)
        {
            _map = new Dictionary<Type, IMeleeComboLengthStrategy>(strategies.Count);
            for (int i = 0; i < strategies.Count; i++)
            {
                IMeleeComboLengthStrategy s = strategies[i];
                _map[s.DefType] = s;
            }
        }

        public UniTask<float> ResolveSeconds(SkillExecutionContext ctx, ISequenceLengthDef lengthDef, CancellationToken token)
        {
            if (lengthDef == null)
            {
                Debug.Log("lengthDef is null");
                return UniTask.FromResult(0f);
            }

            IMeleeComboLengthStrategy strategy;
            if (_map.TryGetValue(lengthDef.GetType(), out strategy) == false)
                throw new InvalidOperationException($"No length strategy for {lengthDef.GetType().Name}");

            return strategy.ResolveSeconds(ctx, lengthDef, token);
        }

    }
}