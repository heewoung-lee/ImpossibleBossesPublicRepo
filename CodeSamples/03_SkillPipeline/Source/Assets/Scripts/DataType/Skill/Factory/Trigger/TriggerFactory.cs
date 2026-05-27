using System;
using System.Collections.Generic;
using System.Linq;
using Character.Skill;
using UnityEngine;
using Util;
using Zenject;

namespace DataType.Skill.Factory.Trigger
{
    public interface ITriggerFactory
    {
        ISkillTriggerStrategy GetTrigger(ITriggerDef triggerDef);
    }

    public sealed class TriggerFactory : ITriggerFactory
    {
        private readonly Dictionary<Type, ISkillTriggerStrategy> _map;

        [Inject]
        public TriggerFactory(List<ISkillTriggerStrategy> strategies)
        {
            _map = strategies.ToDictionary(t => t.DefType, t => t);
        }

        public ISkillTriggerStrategy GetTrigger(ITriggerDef triggerDef)
        {
            if (triggerDef == null)
            {
                UtilDebug.LogError("[TriggerFactory] triggerDef is null");
                return null;
            }

            Type defType = triggerDef.GetType();
            if (_map.TryGetValue(defType, out var trigger))
                return trigger;

            UtilDebug.LogError($"[TriggerFactory] No trigger for defType: {defType.Name}");
            return null;
        }
    }
}