using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Data.DataType.ItemType.Interface;
using DataType.Item;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ItamData.Interface;
using GameManagers.ItamDataManager.Interface;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using UI.SubItem;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace BehaviourTreeNode.BossGolem.Task
{
    public class DropItems : Action
    {
        private IResourcesServices _resourcesServices;
        private IItemDataManager _itemDataManager;
        private RelayManager _relayManager;


        private IResourcesServices ResourcesServices
        {
            get
            {
                if (_resourcesServices == null)
                {
                    _resourcesServices = GetComponent<BossDependencyHub>().ResourcesServices;
                }
                return _resourcesServices;
            }
        }

        private IItemDataManager ItemDataManager
        {
            get
            {
                if (_itemDataManager == null)
                    _itemDataManager = GetComponent<BossDependencyHub>().ItemDataManager;
                return _itemDataManager;
            }
        }

        private RelayManager RelayManager
        {
            get
            {
                if (_relayManager == null)
                {
                    _relayManager = GetComponent<BossDependencyHub>().RelayManager;
                }
                return _relayManager;
            }
        }

        
        private readonly int _minimumTimeCount = 1;
        private readonly int _maximumTimeCount = 3;
        

        [SerializeField] private int _spwanItemCount;
        private List<int> _timeRandom;
        private int _index;
        private bool _isCallIndex;
        float _elapseTime = 0;
        BehaviorTree _tree;

        GameObject _ngoDropItemBehaviour;
        public override void OnStart()
        {
            base.OnStart();
            _tree = Owner.GetComponent<BehaviorTree>();
            _ngoDropItemBehaviour = ResourcesServices.InstantiateByKey("Prefabs/NGO/NGO_BossDropItemBehaviour");
            RelayManager.SpawnNetworkObj(_ngoDropItemBehaviour);
            _index = 0;
            _isCallIndex = false;

            _timeRandom = new List<int>();
            for (int i = 0; i < _spwanItemCount; i++)
            {
                int randomNumber = Random.Range(_minimumTimeCount, _maximumTimeCount);
                _timeRandom.Add(randomNumber);
            }
        
        }


        public override TaskStatus OnUpdate()
        {
            if(_index >= _timeRandom.Count)
            {
                return TaskStatus.Success;
            }

            if (_elapseTime >= _timeRandom[_index] && _isCallIndex == false)
            {
                _isCallIndex = true;
                _elapseTime = 0;
                _index++;
                SpawnItem();
            }
            _isCallIndex = false;
            _elapseTime += Time.deltaTime;
            return TaskStatus.Running;
        }
        private void SpawnItem()
        {
            if (RelayManager.NetworkManagerEx.IsHost == false) return;

            ItemDataSO spawnItem = ItemDataManager.GetRandomItemData();
            
            if (spawnItem != null)
            {
                IteminfoStruct itemStruct = new IteminfoStruct(spawnItem.itemNumber);
                NetworkObjectReference dropItemBehaviour = RelayManager.GetNetworkObject(_ngoDropItemBehaviour);
                RelayManager.NgoRPCCaller.Spawn_Loot_ItemRpc(itemStruct, Owner.transform.position, addLootItemBehaviour: dropItemBehaviour);
            }
        }
        public override void OnEnd()
        {
            base.OnEnd();
            _elapseTime = 0;
            if (_timeRandom != null)
            {
                _timeRandom.Clear();
                _timeRandom = null;
            }
            _resourcesServices.DestroyObject(_ngoDropItemBehaviour);
        }
    }
}
