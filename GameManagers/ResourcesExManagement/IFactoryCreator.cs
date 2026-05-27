using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameManagers.ResourcesExManagement
{
    public interface IFactoryManager
    {
        public bool TryRegisterFactory(GameObject requestedGameObject, Func<Transform,GameObject> factoryCreator);
        public bool RemoveFactory(GameObject requestedGameObject);
        public bool IsKeyRegistered(GameObject requestedGameObject);
        public bool TryGetCreator(GameObject requestedGameObject, out Func<Transform,GameObject> factoryCreator);
        public Func<Transform,GameObject> GetCreator(GameObject requestedGameObject);
        public IReadOnlyCollection<GameObject> GetRegisteredFactoryPrefabs();
    }
}
