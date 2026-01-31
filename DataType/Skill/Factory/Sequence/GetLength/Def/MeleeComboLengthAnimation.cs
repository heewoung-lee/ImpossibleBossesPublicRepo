using System;
using DataType.Skill.Factory.Sequence.Def;
using DataType.Skill.ShareDataDef;

namespace DataType.Skill.Factory.Sequence.GetLength.Def
{
    [Serializable]
    public sealed class LengthAnimation : ISequenceLengthDef
    {
        public AnimNameRefDef animNameRef;
    }
}