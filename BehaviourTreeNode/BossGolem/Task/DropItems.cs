using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataType;
using DataType.Item;
using GameManagers.ItemDataManagement.Interface;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using UI.SubItem;
using Unity.Netcode;
using UnityEngine;

namespace BehaviourTreeNode.BossGolem.Task
{
    [TaskCategory("CustomNode")]
    public class DropItems : Action
    {
        private enum DropCategory
        {
            Equipment,
            Consumable,
            Coin
        }

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
        

        [SerializeField] private float _startDropDelaySeconds;
        [SerializeField] private int _spwanItemCount;
        
        [SerializeField] private float _consumableDropWeight = 1f;
        [SerializeField] private float _coinDropWeight = 1f;
        
        
        [SerializeField] private float _equipmentDropWeight = 1f;
        [SerializeField] private float _normalEquipmentWeight = 1f;
        [SerializeField] private float _magicEquipmentWeight = 1f;
        [SerializeField] private float _rareEquipmentWeight = 1f;
        [SerializeField] private float _uniqueEquipmentWeight = 1f;
        [SerializeField] private float _epicEquipmentWeight = 1f;
        private List<int> _timeRandom;
        private int _index;
        private bool _isCallIndex;
        private bool _canStartDrop;
        private Coroutine _startDropDelayCoroutine;
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
            _canStartDrop = _startDropDelaySeconds <= 0f;
            _elapseTime = 0f;

            _timeRandom = new List<int>();
            for (int i = 0; i < _spwanItemCount; i++)
            {
                int randomNumber = Random.Range(_minimumTimeCount, _maximumTimeCount);
                _timeRandom.Add(randomNumber);
            }

            if (_canStartDrop)
            {
                return;
            }

            _startDropDelayCoroutine = _tree.StartCoroutine(StartDropSequenceAfterDelay());
        }


        public override TaskStatus OnUpdate()
        {
            if (_canStartDrop == false)
            {
                return TaskStatus.Running;
            }

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

        private IEnumerator StartDropSequenceAfterDelay()
        {
            yield return new WaitForSeconds(_startDropDelaySeconds);
            _startDropDelayCoroutine = null;
            _canStartDrop = true;
        }

        private void SpawnItem()
        {
            if (RelayManager.NetworkManagerEx.IsHost == false) return;

            ItemDataSO spawnItem = GetWeightedDropItem();
            SpawnLootItem(spawnItem);
        }

        private void SpawnLootItem(ItemDataSO itemData)
        {
            if (itemData == null)
                return;

            IteminfoStruct itemStruct = new IteminfoStruct(itemData.itemNumber);
            NetworkObjectReference dropItemBehaviour = RelayManager.GetNetworkObject(_ngoDropItemBehaviour);
            RelayManager.NgoRPCCaller.Spawn_Loot_ItemRpc(itemStruct, Owner.transform.position, addLootItemBehaviour: dropItemBehaviour);
        }

        private ItemDataSO GetWeightedDropItem()
        {
            switch (GetWeightedDropCategory())
            {
                case DropCategory.Equipment:
                    return GetRandomDroppableItemData(ItemType.Equipment, GetWeightedEquipmentGrade());
                case DropCategory.Consumable:
                    return GetRandomDroppableItemData(ItemType.Consumable);
                case DropCategory.Coin:
                    return GetRandomDroppableItemData(ItemType.ETC);
                default:
                    return null;
            }
        }

        private ItemDataSO GetRandomDroppableItemData(ItemType type)
        {
            List<ItemDataSO> itemDataList = ItemDataManager.GetItemDataList(type);
            for (int i = itemDataList.Count - 1; i >= 0; i--)
            {
                if (itemDataList[i] is ICanDrop == false)
                {
                    itemDataList.RemoveAt(i);
                }
            }

            if (itemDataList.Count == 0)
                return null;

            return itemDataList[Random.Range(0, itemDataList.Count)];
        }

        private ItemDataSO GetRandomDroppableItemData(ItemType type, ItemGradeType grade)
        {
            List<ItemDataSO> itemDataList = ItemDataManager.GetItemDataList(type);
            for (int i = itemDataList.Count - 1; i >= 0; i--)
            {
                if (itemDataList[i] is ICanDrop == false || itemDataList[i].itemGrade != grade)
                {
                    itemDataList.RemoveAt(i);
                }
            }

            if (itemDataList.Count == 0)
                return null;

            return itemDataList[Random.Range(0, itemDataList.Count)];
        }

        private DropCategory GetWeightedDropCategory()
        {
            float totalWeight = _equipmentDropWeight + _consumableDropWeight + _coinDropWeight;
            if (totalWeight <= 0f)
                return DropCategory.Consumable;

            float roll = Random.Range(0f, totalWeight);
            if (roll < _equipmentDropWeight)
                return DropCategory.Equipment;

            roll -= _equipmentDropWeight;
            if (roll < _consumableDropWeight)
                return DropCategory.Consumable;

            return DropCategory.Coin;
        }

        private ItemGradeType GetWeightedEquipmentGrade()
        {
            float totalWeight =
                _normalEquipmentWeight +
                _magicEquipmentWeight +
                _rareEquipmentWeight +
                _uniqueEquipmentWeight +
                _epicEquipmentWeight;

            if (totalWeight <= 0f)
                return ItemGradeType.Normal;

            float roll = Random.Range(0f, totalWeight);
            if (roll < _normalEquipmentWeight)
                return ItemGradeType.Normal;

            roll -= _normalEquipmentWeight;
            if (roll < _magicEquipmentWeight)
                return ItemGradeType.Magic;

            roll -= _magicEquipmentWeight;
            if (roll < _rareEquipmentWeight)
                return ItemGradeType.Rare;

            roll -= _rareEquipmentWeight;
            if (roll < _uniqueEquipmentWeight)
                return ItemGradeType.Unique;

            return ItemGradeType.Epic;
        }
        public override void OnEnd()
        {
            base.OnEnd();
            if (_startDropDelayCoroutine != null)
            {
                _tree.StopCoroutine(_startDropDelayCoroutine);
                _startDropDelayCoroutine = null;
            }

            _canStartDrop = false;
            _elapseTime = 0;
            if (_timeRandom != null)
            {
                _timeRandom.Clear();
                _timeRandom = null;
            }

            if (_ngoDropItemBehaviour != null)
            {
                _resourcesServices.DestroyObject(_ngoDropItemBehaviour);
                _ngoDropItemBehaviour = null;
            }
        }
    }
}
