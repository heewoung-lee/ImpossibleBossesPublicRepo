using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using DataType.Skill.Factory.Decorator.Def;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory.Decorator
{
    public sealed class DecoratorStackStrategy : IDecoratorStrategy
    {
        public Type DefType => typeof(DecoratorStackDef);


        private readonly Dictionary<Type, IStackElementDecoratorStrategy> _map;

        public DecoratorStackStrategy(List<IStackElementDecoratorStrategy> strategies)
        {
            _map = strategies.ToDictionary(s => s.DefType, s => s);
        }

        public IDecoratorModule Create(IDecoratorDef def, BaseController owner)
        {
            return new Module((DecoratorStackDef)def, owner, _map);
        }


        private sealed class Module : IDecoratorModule
        {
            private readonly IDecoratorModule[] _start;
            private readonly IDecoratorModule[] _tick;
            private readonly IDecoratorModule[] _end;

            private bool _finished;

            public Module(DecoratorStackDef def, BaseController owner, Dictionary<Type, IStackElementDecoratorStrategy> map)
            {
                _start = CreateModules(def.onStart, owner, map);
                _tick  = CreateModules(def.onTick, owner, map);
                _end = CreateModules(def.onEnd, owner, map);
            }

            public void Run(DecoratorPhase phase, SkillExecutionContext ctx, Action onComplete, Action onCancel)
            {
                if (_finished)
                {
                    onComplete();
                    return;
                }

                IDecoratorModule[] list = GetList(phase);
                RunSequential(list, 0, phase, ctx, onComplete, onCancel);
            }

            
            /// <summary>
            /// 1.23일 추가 스킬시전을 마친 스킬들의 초기화를 위한 작업
            /// </summary>
            public void Release()
            {
                foreach (IDecoratorModule startDeco in _start)
                {
                    startDeco.Release();
                }
                foreach (IDecoratorModule tickDeco in _tick)
                {
                    tickDeco.Release();
                }
                foreach (IDecoratorModule endDeco in _end)
                {
                    endDeco.Release();
                }
            }

            private IDecoratorModule[] GetList(DecoratorPhase phase)
            {
                if (phase == DecoratorPhase.Start) return _start;
                if (phase == DecoratorPhase.Tick) return _tick;
                if (phase == DecoratorPhase.End) return _end;
                Debug.Assert(false,$"{phase} is not supported");
                return Array.Empty<IDecoratorModule>();
            }

            private static IDecoratorModule[] CreateModules(
                IDecoratorDef[] defs,
                BaseController owner,
                Dictionary<Type, IStackElementDecoratorStrategy> map)
            {
                if (defs == null || defs.Length == 0)
                    return Array.Empty<IDecoratorModule>();

                List<IDecoratorModule> modules = new List<IDecoratorModule>(defs.Length);

                for (int i = 0; i < defs.Length; i++)
                {
                    IDecoratorDef decoratorDef = defs[i];
                    if (decoratorDef == null) continue;

                    IStackElementDecoratorStrategy strategy;
                    if (map.TryGetValue(decoratorDef.GetType(), out strategy) == false)
                        throw new InvalidOperationException($"No decorator strategy for {decoratorDef.GetType().Name}");

                    modules.Add(strategy.Create(decoratorDef, owner));
                }

                return modules.ToArray();
            }

            private void RunSequential(IDecoratorModule[] list, int index, DecoratorPhase phase, SkillExecutionContext ctx, Action onComplete, Action onCancel)
            {
                if (list == null || index >= list.Length)
                {
                    onComplete();
                    return;
                }

                list[index].Run(phase, ctx,OnComplete,OnCancel);
                void OnComplete()
                {
                    RunSequential(list, index + 1, phase, ctx, onComplete, onCancel);
                }
                void OnCancel()
                {
                    //_finished = true;
                    //1.21일 수정 이부분은 고려할 필요가 있음
                    //캔슬이 됐을 때 모든 모듈을 폐기할것인지 아닌지 
                    onCancel?.Invoke();
                }
                
            }
        }
    }
}
