using System;
using System.Collections.Generic;
using GameManagers.Interface.ResourcesManager;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameManagers.ResourcesEx.implementation
{
    public class ZenjectFactoryManager : IFactoryManager
    {
        private readonly Dictionary<GameObject,Func<Transform,GameObject>> _factoryCreator;

        public ZenjectFactoryManager()
        {
            _factoryCreator = new Dictionary<GameObject, Func<Transform, GameObject>>();
        }
        public bool IsKeyRegistered(GameObject requestedGameObject)
        {
           return _factoryCreator.ContainsKey(requestedGameObject);
        }

        public bool TryGetCreator(GameObject requestedGameObject, out Func<Transform, GameObject> factoryCreator)
        {
            return _factoryCreator.TryGetValue(requestedGameObject, out factoryCreator);
        }

        public Func<Transform, GameObject> GetCreator(GameObject requestedGameObject)
        {
            return _factoryCreator[requestedGameObject];
        }

        public bool TryRegisterFactory(GameObject requestedGameObject, Func<Transform, GameObject> factoryCreator)
        {
           return _factoryCreator.TryAdd(requestedGameObject, factoryCreator);
        }

        public bool RemoveFactory(GameObject requestedGameObject)
        {
            Assert.IsNotNull(_factoryCreator[requestedGameObject],$"{requestedGameObject.name} haven't been registered");
            return _factoryCreator.Remove(requestedGameObject);
        }
        
    }
}
