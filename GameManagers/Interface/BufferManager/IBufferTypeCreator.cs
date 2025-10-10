using System;
using Buffer;

namespace GameManagers.Interface.BufferManager
{
    public interface IBufferTypeCreator
    {
        public BuffModifier CreateBufferType(Type bufferType);
    }
}

