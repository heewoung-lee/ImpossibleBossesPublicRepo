using System;
using System.Collections.Generic;
using GameManagers.Interface.ResourcesManager;
using ProjectContextInstaller;
using Scene.CommonInstaller;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace GameManagers.ResourcesEx
{
    public class CachingObjectDictManager : ICachingObjectDict, IDisposable
    {
        private readonly IRegistrar<ICachingObjectDict> _registrar;
        private readonly Dictionary<string, GameObject> _cachingobjectDict = new Dictionary<string, GameObject>();

        public CachingObjectDictManager(
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IRegistrar<ICachingObjectDict> registrar)
        {
            _registrar = registrar;
            _registrar.Register(this);
        }
        public void Dispose()
        {
            _registrar.Unregister(this);
        }
        
        public bool TryGet(string key, out GameObject go)
        {
            if (_cachingobjectDict.TryGetValue(key, out go) == true)
            {
                return true;
            }

            return false;
        }

        public void AddData(string key, GameObject go)
        {
            _cachingobjectDict.Add(key, go);
            UtilDebug.Log($"CachingDict called AddData {go.name}");
        }

        public void OverwriteData(string key, GameObject go)
        {
            Debug.Assert(go != null, $"{go.name} is null check the key");
            _cachingobjectDict[key] = go;
        }
    }
}