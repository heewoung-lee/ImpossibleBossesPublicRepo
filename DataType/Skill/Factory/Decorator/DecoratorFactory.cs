using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace DataType.Skill.Factory.Decorator
{
    public interface IDecoratorFactory
    {
        IDecoratorStrategy GetDecorator(IDecoratorDef decoratorDef);
    }
    public sealed class DecoratorFactory : IDecoratorFactory
    {
        private readonly Dictionary<Type, IDecoratorStrategy> _map;
        
        
        [Inject]
        public DecoratorFactory(List<IDecoratorStrategy> strategies)
        {
            _map = strategies.ToDictionary(t => t.DefType, t => t);
        }

        public IDecoratorStrategy GetDecorator(IDecoratorDef decoratorDef)
        {
            if (decoratorDef == null)
            {
                Debug.LogError("[DecoratorFactory] decoratorDef is null");
                return null;
            }

            var defType = decoratorDef.GetType();
            if (_map.TryGetValue(defType, out var decorator))
                return decorator;

            Debug.LogError($"[DecoratorFactory] No decorator for defType: {defType.Name}");
            return null;
        }
    }
}