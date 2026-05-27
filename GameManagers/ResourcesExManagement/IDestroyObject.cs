using UnityEngine;

namespace GameManagers.ResourcesExManagement
{
    public interface IDestroyObject
    {
        public void DestroyObject(GameObject go,float delay = 0f);
    }
}