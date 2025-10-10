using Unity.Netcode;
using UnityEngine;

namespace NetWork.BaseNGO
{
    [RequireComponent(typeof(NetworkObject))]
    public abstract class NgoInitializeBase : NetworkBehaviour
    {
        public abstract NetworkObject ParticleNgo { get; }
        public abstract void SetInitialize(NetworkObject particleObj);
    }
}
