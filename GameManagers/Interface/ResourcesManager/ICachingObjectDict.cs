using UnityEngine;

namespace GameManagers.Interface.ResourcesManager
{
    public interface ICachingObjectDict<in TKey>
    {
        public bool TryGet(TKey key, out GameObject go);
        public void AddData(TKey key, GameObject go);
        public void OverwriteData(TKey key, GameObject go);
    }
}
