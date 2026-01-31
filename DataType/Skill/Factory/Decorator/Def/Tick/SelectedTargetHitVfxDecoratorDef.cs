using System;

namespace DataType.Skill.Factory.Decorator.Def.Tick
{
    [Serializable]
    public sealed class SelectedTargetHitVfxDecoratorDef : ITickDecoratorDef
    {
        public HitVfxDecoratorDef hitVfx;
    }
}