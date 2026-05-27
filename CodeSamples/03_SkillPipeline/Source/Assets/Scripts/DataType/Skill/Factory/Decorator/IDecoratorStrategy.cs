using System;
using Controller;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Effect;
using DataType.Skill.Factory.Target;
using Skill;

namespace DataType.Skill.Factory.Decorator
{
    public interface IDecoratorStrategy
    {
        Type DefType { get; }
        IDecoratorModule Create(IDecoratorDef def, BaseController owner);
    }

    public interface IDecoratorModule
    {
        void Run(DecoratorPhase phase, SkillExecutionContext ctx, Action onComplete, Action onCancel);
        
        void Release();
        //1.23일 추가 데코레이터가 끝난다음에 모듈을 초기화할 곳은 여기에 초기화
    }
}