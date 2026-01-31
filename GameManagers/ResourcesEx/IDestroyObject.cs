using UnityEngine;

namespace GameManagers.ResourcesEx
{
    public interface IDestroyObject
    {
        public void DestroyObject(GameObject go,float delay = 0f);
    }
}