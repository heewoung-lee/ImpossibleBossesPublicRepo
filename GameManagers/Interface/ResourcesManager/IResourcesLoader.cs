
namespace GameManagers.Interface.ResourcesManager
{
    public interface IResourcesLoader<in TKey>
    {
        public T Load<T>(TKey key) where T : UnityEngine.Object;
        public T[] LoadAll<T>(TKey key) where T : UnityEngine.Object;
        public bool TryGetLoad<T>(TKey key, out T loadItem) where T : UnityEngine.Object;
    }
}