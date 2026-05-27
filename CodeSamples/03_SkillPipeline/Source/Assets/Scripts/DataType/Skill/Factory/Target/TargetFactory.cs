using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;
using Zenject;

namespace DataType.Skill.Factory.Target
{
    public interface ITargetingFactory
    {
        ITargetingStrategy GetTargeting(ITargetingDef targetingDef);
    }

    public sealed class TargetFactory : ITargetingFactory
    {
        private readonly Dictionary<Type, ITargetingStrategy> _map;

        [Inject]
        public TargetFactory(List<ITargetingStrategy> strategies)
        {
            _map = strategies.ToDictionary(t => t.DefType, t => t);
        }

        public ITargetingStrategy GetTargeting(ITargetingDef targetingDef)
        {
            if (targetingDef == null)
            {
                UtilDebug.LogError("[TargetingFactory] targetingDef is null");
                return null;
            }

            var defType = targetingDef.GetType();
            if (_map.TryGetValue(defType, out var targeting))
                return targeting;

            UtilDebug.LogError($"[TargetingFactory] No targeting for defType: {defType.Name}");
            return null;
        }
    }
}