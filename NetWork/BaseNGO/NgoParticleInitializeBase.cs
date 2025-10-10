using System;
using GameManagers;
using GameManagers.Interface.VFXManager;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace NetWork.BaseNGO
{
    [RequireComponent(typeof(NetworkObject))]
    public abstract class NgoParticleInitializeBase : NetworkBehaviour
    {
        public abstract NetworkObject ParticleNgo { get; }
        public abstract void SetInitialize(NetworkObject particleObj);
        
        [Inject] private IVFXManagerServices _vfxManager;
        public abstract NetworkObject TargetNgo { get; }
        public abstract void SetTargetInitialize(NetworkObject targetNgo);

        public virtual void StartParticleOption(Action<GameObject> callback)
        {
            callback?.Invoke(gameObject);
        }
    }
}
