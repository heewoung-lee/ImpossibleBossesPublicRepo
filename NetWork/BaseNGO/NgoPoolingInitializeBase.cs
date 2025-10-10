using System;
using GameManagers;
using GameManagers.Interface.PoolManager;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace NetWork.BaseNGO
{
    [RequireComponent(typeof(Poolable))]
    public abstract class NgoPoolingInitializeBase : NgoParticleInitializeBase, INgoPooldata
    {
        private NgoPoolManager _poolManager;

        [Inject] 
        private void Construct(NgoPoolManager poolManager)
        {
            _poolManager = poolManager;
        }

        public abstract string PoolingNgoPath { get; }
        public abstract int PoolingCapacity { get; }
        
        private NetworkObject _particleNgo;
        private NetworkObject _targetNgo;

        public override NetworkObject TargetNgo => _targetNgo;
        public override NetworkObject ParticleNgo => _particleNgo;

        private Action _poolObjectReleaseEvent;
        private Action _poolObjectGetEvent;

        public event Action PoolObjectReleaseEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _poolObjectReleaseEvent,value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _poolObjectReleaseEvent, value);

            }
        }
        public event Action PoolObjectGetEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _poolObjectGetEvent,value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _poolObjectGetEvent, value);
            }
        }

        public override void SetInitialize(NetworkObject particleObj)
        {
            _particleNgo = particleObj;
        }
        public override void SetTargetInitialize(NetworkObject targetNgo)
        {
            _targetNgo = targetNgo;
        }
        public virtual void OnPoolGet()
        {
            _poolObjectGetEvent?.Invoke();
        }
        public virtual void OnPoolRelease()
        {
            _poolObjectReleaseEvent?.Invoke();
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            PoolObjectSetParent();
        }
        private void PoolObjectSetParent()
        {
            if(_poolManager.PoolNgoRootDict.TryGetValue(PoolingNgoPath,out Transform parentTr))
            {
                transform.SetParent(parentTr,false);
            }
        }
    }
}