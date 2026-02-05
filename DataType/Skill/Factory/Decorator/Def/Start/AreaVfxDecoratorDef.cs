using System;
using DataType.Skill.ShareDataDef;
using Sirenix.OdinInspector;

namespace DataType.Skill.Factory.Decorator.Def.Start
{
    [Serializable]
    public class AreaVfxDecoratorDef : INormalDecoratorDef
    {
        public HitVfxDecoratorDef vfxInfo;
        public ScaleRefDef scaleRef;
    }
}