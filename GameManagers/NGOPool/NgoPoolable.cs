using System;
using GameManagers.Pool;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.PoolManager
{
    [RequireComponent(typeof(NetworkObject))]
    public class NgoPoolable : Poolable
    {
        private NgoPoolManager _ngoPoolManager;
        private string _ngoPoolPath;
        private NetworkObject _networkObject;
        private INgoPooldata _pooldata;
        [Inject]
        public void Construct(NgoPoolManager ngoPoolManager)
        {
            _ngoPoolManager = ngoPoolManager;
            
            _pooldata = GetComponent<INgoPooldata>();
            Debug.Assert(_pooldata != null, "You should Write NgoPoolingInitialize And Attach it");

            _ngoPoolPath = _pooldata.PoolingNgoPath;
            Debug.Assert(!string.IsNullOrEmpty(_ngoPoolPath), "PoolingNgoPath is null/empty");

            _networkObject = GetComponent<NetworkObject>();
            Debug.Assert(_networkObject != null, "NetworkObject missing");
        }
        public override GameObject Pop()
        {
            return _ngoPoolManager.Pop(_ngoPoolPath).gameObject;
        }
        public override void Push()
        {
            _ngoPoolManager.Push(_networkObject);
        }
    }
}
