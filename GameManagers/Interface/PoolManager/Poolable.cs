using UnityEngine;
using Zenject;

namespace GameManagers
{
    [DisallowMultipleComponent]
    public abstract class Poolable : MonoBehaviour
    {
        public bool IsUsing { get; set; }
        public bool WorldPositionStays { get; set; } = true;
        public abstract GameObject Pop();
        public abstract void Push();

    }
}