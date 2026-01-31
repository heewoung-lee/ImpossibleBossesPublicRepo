using System;
using DataType.Skill.Factory.Sequence.Def;
using DataType.Skill.ShareDataDef;

namespace DataType.Skill.Factory.Sequence.GetLength.Def
{
    [Serializable]
    public sealed class LengthPrefabLife : ISequenceLengthDef
    {
        public VFXPathRefDef vfxPathDef = new VFXPathRefDef();  // 예: VFX/Hitbox 프리팹 경로
    }
}