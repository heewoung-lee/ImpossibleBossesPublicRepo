using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface.ItemDataManager;
using GameManagers.Interface.ResourcesManager;
using UI.SubItem;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace BehaviourTreeNode.BossGolem.Task
{
    public class DropItems : Action
    {
        private IResourcesServices _resourcesServices;
        private IItemGetter _itemGetter;
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

        private IItemGetter ItemGetter
        {
            get
            {
                if (_itemGetter == null)
                {
                    _itemGetter =  GetComponent<BossDependencyHub>().ItemGetter;
                }
                return _itemGetter;
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

                void SpawnItem()
                {
                    if (RelayManager.NetworkManagerEx.IsHost == false)
                        return;

                    IItem spawnItem = ItemGetter.GetRandomItemFromAll();
                    IteminfoStruct itemStruct = new IteminfoStruct(spawnItem);
                    NetworkObjectReference dropItemBehaviour = RelayManager.GetNetworkObject(_ngoDropItemBehaviour);
                    RelayManager.NgoRPCCaller.Spawn_Loot_ItemRpc(itemStruct, Owner.transform.position, addLootItemBehaviour:dropItemBehaviour);
                }
            }
            _isCallIndex = false;
            _elapseTime += Time.deltaTime;
            return TaskStatus.Running;
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
            RelayManager.DeSpawn_NetWorkOBJ(_ngoDropItemBehaviour);
        }
    }
}
