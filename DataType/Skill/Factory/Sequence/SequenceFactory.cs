using System;
using System.Collections.Generic;
using System.Linq;

namespace DataType.Skill.Factory.Sequence
{
    public interface ISequenceFactory
    {
        ISequenceStrategy GetSequence(ISequenceDef def);
    }

    public sealed class SequenceFactory : ISequenceFactory
    {
        private readonly Dictionary<Type, ISequenceStrategy> _map;

        public SequenceFactory(List<ISequenceStrategy> strategies)
        {
            if (strategies == null)
                _map = new Dictionary<Type, ISequenceStrategy>();
            else
                _map = strategies.ToDictionary(s => s.DefType, s => s);
        }

        public ISequenceStrategy GetSequence(ISequenceDef def)
        {
            if (def == null) return null;

            Type type = def.GetType();
            ISequenceStrategy start;
            if (_map.TryGetValue(type, out start))
                return start;

            return null;
        }
    }
}