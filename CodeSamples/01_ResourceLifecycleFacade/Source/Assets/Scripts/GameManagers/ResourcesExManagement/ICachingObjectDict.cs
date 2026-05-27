using UnityEngine;

namespace GameManagers.ResourcesExManagement
{
    public interface ICachingObjectDict
    {
        public bool TryGet(string key, out GameObject go);
        public void AddData(string key, GameObject go);
        public void OverwriteData(string key, GameObject go);
    }
}
