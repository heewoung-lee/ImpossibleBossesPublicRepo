using System;
using Controller.CrowdControl;

namespace DataType.Skill.Factory.Effect.Def
{
    [Serializable]
    public class CrowdControlDef : IEffectDef
    {
        public CCType ccType;
        public DurationRefDef hitVfxDuration = new DurationRefDef();
    }
}
