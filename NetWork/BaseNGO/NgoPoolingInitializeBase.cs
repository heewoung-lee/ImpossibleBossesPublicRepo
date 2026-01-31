using System;
using GameManagers;
using GameManagers.Interface.PoolManager;
using GameManagers.Pool;
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
        public override NetworkObject ParticleNgo => _particleNgo;

        private Action _poolObjectReleaseEvent;
        private Action _poolObjectGetEvent;
        private ulong _targetObjectId = ulong.MaxValue;
        public ulong TargetObjectId => _targetObjectId;
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

        //타겟이 정해져있는 파티클로직
        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            _vfxManager.SetParticlePosAndLifeCycle(gameObject,PoolingNgoPath,duration);
            
            //1.5수정 파티클 제거 로직 추가
            //기존에는 StartParticle옵션에서 커스터마이징 해서 파티클의 제거를 직접넣었지만
            //파티클의 양이 많아 짐에 따라 자동으로 제거 로직을 넣고 다른 커스터마이징을 startParticle에서
            //진행하는방향이 맞는것 같아 수정함;
        }
        
        
        //NetworkParams을 받는 파티클 시작 로직
        public override void StartParticleOption(float duration,NetworkParams networkParams)
        {
            base.StartParticleOption(duration,networkParams);
            _vfxManager.SetParticlePosAndLifeCycle(gameObject,PoolingNgoPath,duration);
            
        }
       
        
        
        [Rpc(SendTo.ClientsAndHost)]
        public void InitializeVfxClientRpc(ulong targetObjectId, float duration)
        {
            _targetObjectId = targetObjectId;
            // 1. 자기 자신 초기화 (이미 Spawn된 상태임이 보장됨)
            SetInitialize(this.NetworkObject); 
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetNgo))
            {
                transform.position = targetNgo.transform.position;
                // 3. 파티클 실행 로직
                StartParticleOption(targetNgo.gameObject, duration);
                
            }
          
        }
        [Rpc(SendTo.ClientsAndHost)]
        public void InitializeVfxClientRpc(float duration,NetworkParams networkParams)
        {
            SetInitialize(this.NetworkObject); 
            StartParticleOption(duration,networkParams);
        }
        
    }
}