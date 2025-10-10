using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.PoolManager
{
    public class NgoPoolable : Poolable
    {
        private NgoPoolManager _ngoPoolManager;
        private string _ngoPoolPath;
        private NetworkObject _networkObject;

        [Inject]
        public void Construct(NgoPoolManager ngoPoolManager)
        {
            _ngoPoolManager = ngoPoolManager;
            INgoPooldata pooldata = GetComponent<INgoPooldata>();
            _ngoPoolPath = pooldata.PoolingNgoPath;
            _networkObject = GetComponent<NetworkObject>();
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
