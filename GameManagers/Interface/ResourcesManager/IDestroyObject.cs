using UnityEngine;

namespace GameManagers.Interface.ResourcesManager
{
    public interface IDestroyObject
    {
        public void DestroyObject(GameObject go,float delay = 0f);
    }
}