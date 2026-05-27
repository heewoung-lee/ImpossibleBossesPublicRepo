using System;
using UnityEngine;

namespace DataType.Skill.Factory.Decorator.Def
{
    public enum DecoratorPhase { Start, Tick, End}
    [Serializable]
    public sealed class DecoratorStackDef : IDecoratorDef
    {
        [SerializeReference] public INormalDecoratorDef[] onStart = new INormalDecoratorDef[0];
        [SerializeReference] public ITickDecoratorDef[]   onTick  = new ITickDecoratorDef[0];
        [SerializeReference] public INormalDecoratorDef[] onEnd = new INormalDecoratorDef[0];
    }

}