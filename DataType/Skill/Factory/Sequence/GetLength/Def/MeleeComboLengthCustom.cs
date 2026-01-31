using System;
using DataType.Skill.Factory.Sequence.Def;
using Sirenix.OdinInspector;

namespace DataType.Skill.Factory.Sequence.GetLength.Def
{
    [Serializable]
    public sealed class LengthCustom : ISequenceLengthDef
    {
        [MinValue(0.1f)]
        public float seconds = 1f;
    }
    
}