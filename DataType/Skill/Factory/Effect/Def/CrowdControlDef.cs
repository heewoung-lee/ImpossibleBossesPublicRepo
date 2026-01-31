using System;

namespace DataType.Skill.Factory.Effect.Def
{
    public enum CCType
    {
        Taunt
    }
    
    [Serializable]
    public class CrowdControlDef: IEffectDef
    {
        public CCType ccType;
        public float Value { get; }
    }
}