using System;
using UnityEngine;

namespace GameManagers.Interface.ResourcesManager
{
    public interface IFactoryCreator
    {
        public bool IsKeyRegistered(GameObject requestedGameObject);
        public bool TryGetCreator(GameObject requestedGameObject, out Func<Transform,GameObject> factoryCreator);
        public Func<Transform,GameObject> GetCreator(GameObject requestedGameObject);
    }

    public interface IFactoryRegister
    {
        public bool TryRegisterFactory(GameObject requestedGameObject, Func<Transform,GameObject> factoryCreator);
        
        public bool RemoveFactory(GameObject requestedGameObject);
    }
}
