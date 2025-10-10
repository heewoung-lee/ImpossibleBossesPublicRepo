using UnityEngine;

namespace GameManagers.Interface.ResourcesManager.implementation
{
    public class NullLoader<TKey> :  IResourcesLoader<TKey>
    {
        public T Load<T>(TKey key) where T : Object
        {
            return Resources.Load<T>(key.ToString());
        }

        public T[] LoadAll<T>(TKey key) where T : Object
        {
            return Resources.LoadAll<T>(key.ToString());
        }

        public bool TryGetLoad<T>(TKey key, out T loadItem) where T : Object
        {
            loadItem = Load<T>(key);

            if (loadItem == null)
                return false;
            else
                return true;
        }
    }
}