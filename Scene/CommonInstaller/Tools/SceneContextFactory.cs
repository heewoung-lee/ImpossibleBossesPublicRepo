using System;
using System.Collections.Generic;
using System.Linq;
using GameManagers.Interface.ResourcesManager;
using NUnit.Framework;
using ProjectContextInstaller;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace Scene.CommonInstaller.Tools
{
    public class SceneContextFactory: IFactoryCreator,IDisposable
    {
        private readonly Dictionary<GameObject,Func<Transform,GameObject>> _factoryCreator;
        private readonly IRegistrar<IFactoryCreator> _iFactoryCreatorRegistrar;
        private readonly IFactoryController _factoryController;

        private IEnumerable<IRegisteredFactoryObject> _requestObjectList;

        public SceneContextFactory(IEnumerable<IRegisteredFactoryObject> iRegisteredFactoryObjects,
            [Inject(Id = FactoryControllerInstaller.FactoryBindKey)]
            IRegistrar<IFactoryCreator>iFactoryCreatorRegistrar, IFactoryController factoryController)
        {
            _requestObjectList = iRegisteredFactoryObjects;
            _iFactoryCreatorRegistrar = iFactoryCreatorRegistrar;
            _factoryController = factoryController;
            
            _factoryCreator = _requestObjectList.ToDictionary(factoryType => factoryType.RequestObject, factoryCreator => factoryCreator.Creator);
            _iFactoryCreatorRegistrar.Register(this);
        }
        public void Dispose()
        {
            foreach (IRegisteredFactoryObject registeredObj in _requestObjectList)
            {
                _factoryController.RemoveFactory(registeredObj.RequestObject);
            }
            _iFactoryCreatorRegistrar.Unregister(this);
        }

        public bool IsKeyRegistered(GameObject requestedGameObject)
        {
           return _factoryCreator.ContainsKey(requestedGameObject);
        }

        public bool TryGetCreator(GameObject requestedGameObject, out Func<Transform,GameObject> factoryCreator)
        {
            return _factoryCreator.TryGetValue(requestedGameObject, out factoryCreator);
        }

        public Func<Transform, GameObject> GetCreator(GameObject requestedGameObject)
        {
            return _factoryCreator[requestedGameObject];
        }
    }
}
