using System;
using DataType.Skill.ShareDataDef;
using Sirenix.OdinInspector;

namespace DataType.Skill.Factory.Decorator.Def
{

    [Serializable]
    public sealed class HitVfxDecoratorDef : IDecoratorDef
    {
        public VFXPathRefDef hitVfxPath = new VFXPathRefDef();
        public DurationRefDef hitVfxDuration = new DurationRefDef();
        public bool checkHitVfxLocatedHead = false; //VFX를 머리쪽에 스폰할 것인지
    }
}