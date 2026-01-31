
namespace GameManagers.ResourcesEx
{
    public interface IResourcesLoader
    {
        public T Load<T>(string key) where T : UnityEngine.Object;
        public T[] LoadAll<T>(string key) where T : UnityEngine.Object;
        public bool TryGetLoad<T>(string key, out T loadItem) where T : UnityEngine.Object;
    }
}