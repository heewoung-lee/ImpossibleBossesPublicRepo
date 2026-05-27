using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;
using Zenject;

namespace DataType.Skill.Factory.Effect
{
    public interface IEffectFactory
    {
        IEffectStrategy GetEffect(IEffectDef effectDef);
    }

    public sealed class EffectFactory : IEffectFactory
    {
        private readonly Dictionary<Type, IEffectStrategy> _map;

        [Inject]
        public EffectFactory(List<IEffectStrategy> strategies)
        {
            _map = strategies.ToDictionary(t => t.DefType, t => t);
        }

        public IEffectStrategy GetEffect(IEffectDef effectDef)
        {
            if (effectDef == null)
            {
                UtilDebug.LogError("[EffectFactory] effectDef is null");
                return null;
            }

            var defType = effectDef.GetType();
            if (_map.TryGetValue(defType, out var effect))
                return effect;

            UtilDebug.LogError($"[EffectFactory] No effect for defType: {defType.Name}");
            return null;
        }
    }
}