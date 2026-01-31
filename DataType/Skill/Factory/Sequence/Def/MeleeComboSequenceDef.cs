using System;
using DataType.Skill.Factory.Decorator.Def;
using UnityEngine;

namespace DataType.Skill.Factory.Sequence.Def
{
    public interface ISequenceLengthDef { }


    [Serializable]
    public sealed class MeleeComboSequenceDef : ISequenceDef
    {
        [SerializeReference] public ISequenceLengthDef length;
        public HitEventDef[] hits;
    }
}