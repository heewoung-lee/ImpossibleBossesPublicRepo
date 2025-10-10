using System;
using System.Collections.Generic;
using GameManagers.Interface.ResourcesManager;
using ProjectContextInstaller;
using Scene.CommonInstaller;
using Scene.ZenjectInstaller;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers
{
    public class CachingObjectDictManager<TKey> : ICachingObjectDict<TKey>, IDisposable
    {
        private readonly IRegistrar<ICachingObjectDict<TKey>> _registrar;
        private readonly Dictionary<TKey, GameObject> _cachingobjectDict = new Dictionary<TKey, GameObject>();

        public CachingObjectDictManager(
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IRegistrar<ICachingObjectDict<TKey>> registrar)
        {
            _registrar = registrar;
            _registrar.Register(this);
        }
        public void Dispose()
        {
            _registrar.Unregister(this);
        }
        
        public bool TryGet(TKey key, out GameObject go)
        {
            if (_cachingobjectDict.TryGetValue(key, out go) == true)
            {
                return true;
            }

            return false;
        }

        public void AddData(TKey key, GameObject go)
        {
            _cachingobjectDict.Add(key, go);
            Debug.Log($"CachingDict called AddData {go.name}");
        }

        public void OverwriteData(TKey key, GameObject go)
        {
            Debug.Assert(go != null, $"{go.name} is null check the key");
            _cachingobjectDict[key] = go;
        }
    }
}