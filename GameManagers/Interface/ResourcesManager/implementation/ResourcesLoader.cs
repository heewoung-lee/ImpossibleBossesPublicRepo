using UnityEngine;
using UnityEngine.Assertions;

namespace GameManagers.Interface.ResourcesManager.implementation
{
    public class ResourcesLoader : IResourcesLoader<string>
    {
        public T Load<T>(string key) where T : Object
        {
            T loadObj = Resources.Load<T>(key);
            Assert.IsNotNull(loadObj,$"loadObj != null key: {key}");
            return loadObj;
        }

        public T[] LoadAll<T>(string key) where T : Object
        {
            T[] loadObjs = Resources.LoadAll<T>(key);
            Assert.IsNotNull(loadObjs,$"loadObjs != null key: {key}");
            return Resources.LoadAll<T>(key);
        }

        public bool TryGetLoad<T>(string key, out T loadItem) where T : Object
        {
            loadItem = Load<T>(key);

            if (loadItem == null)
                return false;
            else
                return true;
        }
    }
}
