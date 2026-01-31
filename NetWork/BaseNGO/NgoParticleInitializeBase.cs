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
        [Inject] protected IVFXManagerServices _vfxManager;
        public abstract NetworkObject ParticleNgo { get; }
        public abstract void SetInitialize(NetworkObject particleObj);
        public virtual void StartParticleOption(GameObject targetGo, float duration){}
        public virtual void StartParticleOption(float duration,NetworkParams networkParams){}
    }
}
