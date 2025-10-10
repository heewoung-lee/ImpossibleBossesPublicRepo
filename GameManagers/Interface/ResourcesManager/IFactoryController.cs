using System;
using System.Collections.Generic;
using NUnit.Framework;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.ResourcesManager
{
    public interface IFactoryController
    {
        public bool TryGetCreator(GameObject prefabObj, out Func<Transform,GameObject> factoryCreator);
        public bool TryRegisterGameObjectContextFactory(GameObject requestedGameObject, Func<Transform, GameObject> factoryCreator);
        public bool RemoveFactory(GameObject requestedGameObject);
    }
    
    
    public class FactoryController : IFactoryController
    {
        private readonly IFactoryRegister _gameObjectContextFactoryCreator;
        //private IFactoryCreator _sceneContextFactoryCreator;
        
        private readonly Dictionary<GameObject, IFactoryCreator> _factoryCreators;
        
        [Inject]
        public FactoryController(IFactoryRegister gameObjectContextFactoryCreator)
        {
            _gameObjectContextFactoryCreator = gameObjectContextFactoryCreator;
            _factoryCreators = new Dictionary<GameObject, IFactoryCreator>();
        }

        public bool TryGetCreator(GameObject prefabObj, out Func<Transform, GameObject> factoryCreator)
        {
            //기본 생성오브젝트도 있으니깐 기본생성 오브젝트 걸러야함.
            if (_factoryCreators.ContainsKey(prefabObj) == false)
            {
               
                    //두번째는 gameobjectContext에 등혹됐는지 확인.
                    IFactoryCreator gameObjectContextFactory = _gameObjectContextFactoryCreator as IFactoryCreator;
                    Assert.IsNotNull(gameObjectContextFactory, "gameObjectContextFactory is null");
                    if (gameObjectContextFactory.IsKeyRegistered(prefabObj) == true)
                    {
                        _factoryCreators.Add(prefabObj,gameObjectContextFactory);
                    }
            }

            if (_factoryCreators.TryGetValue(prefabObj, out var creator) == true)
            {
                return creator.TryGetCreator(prefabObj, out factoryCreator);
            }
            
            factoryCreator = null;
            return false;
        }

        public bool TryRegisterGameObjectContextFactory(GameObject requestedGameObject, Func<Transform, GameObject> factoryCreator)
        {
            //bool isFactoryDuplicateRegistered = _sceneContextFactoryCreator.IsKeyRegistered(requestedGameObject);
            //Assert.IsFalse(isFactoryDuplicateRegistered,$"requestedGameObject : {requestedGameObject.name} is duplicate registered");
            return _gameObjectContextFactoryCreator.TryRegisterFactory(requestedGameObject, factoryCreator);
        }

        // public void Register(IFactoryCreator sceneContext)
        // {
        //     _sceneContextFactoryCreator = sceneContext;
        // }
        //
        // public void Unregister(IFactoryCreator sceneContext)
        // {
        //     if (_sceneContextFactoryCreator == sceneContext)
        //     {
        //         _sceneContextFactoryCreator = null;
        //     }
        // }

        public bool RemoveFactory(GameObject requestedGameObject)
        {
            if (((IFactoryCreator)_gameObjectContextFactoryCreator).IsKeyRegistered(requestedGameObject))
            {
                _gameObjectContextFactoryCreator.RemoveFactory(requestedGameObject);
            }
            //_sceneContextFactory는 Remove할 필요가 없다. 씬용이면 씬이 넘어가는순간 전부 빠져버리기 때문에 GC가 자동으로 수거한다.

            return _factoryCreators.Remove(requestedGameObject);
        }
    }
}
