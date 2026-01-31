using UnityEngine;
using Zenject;

namespace GameManagers
{
    public class LootItemManager
    {
        private readonly RelayManager.RelayManager _relayManager;
        
        private GameObject _itemRoot;

        [Inject] 
        public LootItemManager(RelayManager.RelayManager relayManager)
        {
            _relayManager = relayManager;
        }

        public Transform ItemRoot
        {
            get
            {
                if(_itemRoot == null)
                {
                    _itemRoot = _relayManager.SpawnNetworkObj("Prefabs/NGO/ItemRootNetwork");
                }
                return _itemRoot.transform;
            }
        }
    }
}