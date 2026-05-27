namespace DataType.Skill.Factory.Decorator
{
    public interface IDecoratorDef { }
    
    /// <summary>
    /// 스택에 들어가는 마커용 인터페이스
    /// </summary>
    public interface IStackElementDecoratorDef : IDecoratorDef{ }
    
    
    /// <summary>
    /// Start,End스택에 들어가는 마커 인터페이스
    /// </summary>
    public interface INormalDecoratorDef : IStackElementDecoratorDef { }
   
    /// <summary>
   /// 틱에 들어가는 마커 인터페이스
   /// </summary>
    public interface ITickDecoratorDef  : IStackElementDecoratorDef { }
    
}
