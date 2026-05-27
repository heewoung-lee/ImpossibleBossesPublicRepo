using System;
using Controller;

namespace DataType.Skill.Factory.Sequence
{
    public interface ISequenceStrategy
    {
        Type DefType { get; }
        ISequenceModule Create(ISequenceDef def, BaseController owner);
    }
}