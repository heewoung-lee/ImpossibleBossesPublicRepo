using System;
using Buffer;
using Zenject;

namespace GameManagers.Interface.BufferManager.implementation
{
    
    public class BufferTypeCreate : IBufferTypeCreator
    {
        private readonly DiContainer _diContainer;

        [Inject]
        public BufferTypeCreate(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }
        
        public BuffModifier CreateBufferType(Type bufferType)
        {
            BuffModifier modifierInstance = Activator.CreateInstance(bufferType) as BuffModifier;
            _diContainer.Inject(modifierInstance);
            return modifierInstance;
        }
    }
}