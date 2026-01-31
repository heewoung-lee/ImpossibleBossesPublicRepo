using System;
using DataType.Skill.ShareDataDef;
using Sirenix.OdinInspector;

namespace DataType.Skill.Factory.Decorator.Def.Start
{
    [Serializable]
    public sealed class CastVfxDecoratorDef : INormalDecoratorDef
    {
        public VFXPathRefDef castVfxPath = new VFXPathRefDef();
        public DurationRefDef castVfxDuration = new DurationRefDef();
    }
}