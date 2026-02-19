using System;
using System.Collections.Generic;
using System.Linq;
using DataType.Item;
using UnityEngine;
using Util;
using Zenject;

namespace DataType.Skill.Factory
{
    public interface IStrategyFactory
    {
        IStrategy GetStrategy(BaseDataSO data);
    }
    
    public class StrategyFactory : IStrategyFactory
    {
        private readonly Dictionary<Type, IStrategy> _strategyMap;
        // ProjectContext에 등록된 모든 IStrategy 구현체들을 
        // Zenject가 자동으로 리스트로 묶어서 넣어줌
        [Inject]
        public StrategyFactory(List<IStrategy> strategies, SignalBus factoryReadySignal)
        {
            _strategyMap = strategies.ToDictionary(s => s.GetType(), s => s);
        }
        
        public IStrategy GetStrategy(BaseDataSO data)
        {
            if (data is IGetterStrategyType getterStrategyType)
            {
                Type type = getterStrategyType.GetStrategyType();
                if (_strategyMap.TryGetValue(type, out IStrategy rawStrategy))
                {
                    return rawStrategy;
                }
            }
            UtilDebug.LogError($"[Factory] 전략을 찾을 수 없습니다: {data.dataName}");
            return null;
        }
    }
}