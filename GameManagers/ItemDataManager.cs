using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.DataType.ItemType;
using Data.DataType.ItemType.Interface;
using GameManagers.Interface.DataManager;
using GameManagers.Interface.ItemDataManager;
using GameManagers.Interface.ResourcesManager;
using NetWork.LootItem;
using Scene.ZenjectInstaller;
using UnityEngine;
using Util;
using Zenject;
using Random = UnityEngine.Random;
namespace GameManagers
{//전역으로 옮겨야함.
    public class ItemDataManager:IInitializable,IItemGradeBorder,IItemGetter,ILootItemGetter
    {
        [Inject] private IResourcesServices _resourcesServices;
        [Inject] private IAllData _allData;
        [Inject] private IRequestDataType _requestDataType;
        
        
        private const string ItemFrameBorderPath = "Art/UI/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/Frame";
        private Dictionary<ItemGradeType, Sprite> _itemGradeBorder;
        private Dictionary<Type, Dictionary<int, IItem>> _allItemDataDict = new Dictionary<Type, Dictionary<int, IItem>>();
        private Dictionary<int, IItem> _itemDataKeyDict = new Dictionary<int, IItem>();
        private IList<Type> _itemDataType;
        public void Initialize()
        {
            //폴더내에 있는 타입들을 긁어와서 데이터를 읽는다.
            _itemDataType = _requestDataType.LoadSerializableTypesFromFolder("Assets/Scripts/Data/DataType/ItemType"
                , DataUtil.AddSerializableAttributeType);

            foreach (Type itemtype in _itemDataType)
            {
                IDictionary itemTypeIDict = _allData.GetData(itemtype) as IDictionary;
                //IDictionary 인터페이스로 변환한 후 모든 Dictionary데이터를 받아올 수 있도록한다.
                if (itemTypeIDict == null)
                {
                    Debug.Log($"Not Found Type:{itemtype}");
                    continue;
                }

                Dictionary<int, IItem> itemDict = new Dictionary<int, IItem>();
                foreach (int key in itemTypeIDict.Keys)//해당타입의 키 값과 대응 되는 데이터를 캐스팅한 후 딕셔너리에 넣어준다.
                {
                    IItem itemValue = itemTypeIDict[key] as IItem;//키에 대응되는 IItem
                    if (itemValue == null)
                    {
                        Debug.LogError($"Failed to cast item of type {itemtype} to IItem");
                        continue;
                    }
                    itemDict[key] = itemValue;
                    _itemDataKeyDict.Add(key, itemValue);
                }
                _allItemDataDict[itemtype] = itemDict;
                _allItemDataDict[itemtype] = BindImageSources(_allItemDataDict[itemtype]) as Dictionary<int, IItem>;
            }

            _itemGradeBorder = new Dictionary<ItemGradeType, Sprite>//아이템 등급 프레임 초기화
            {
                { ItemGradeType.Normal, _resourcesServices.Load<Sprite>(ItemFrameBorderPath + "/ItemFrame_01_Border_White") },
                { ItemGradeType.Magic, _resourcesServices.Load<Sprite>(ItemFrameBorderPath + "/ItemFrame_01_Border_Green") },
                { ItemGradeType.Rare, _resourcesServices.Load<Sprite>(ItemFrameBorderPath + "/ItemFrame_01_Border_Blue") },
                { ItemGradeType.Unique, _resourcesServices.Load<Sprite>(ItemFrameBorderPath + "/ItemFrame_01_Border_Red") },
                { ItemGradeType.Epic, _resourcesServices.Load<Sprite>(ItemFrameBorderPath + "/ItemFrame_01_Border_Yellow") }
            };
        }
        public Sprite GetGradeBorder(ItemGradeType gradeType)
        {
            if (_itemGradeBorder != null && _itemGradeBorder.TryGetValue(gradeType, out var sprite))
                return sprite;

            Debug.LogWarning($"[GetGradeBorder] 해당 gradeType({gradeType})이 존재하지 않습니다.");
            return null;
        }
        
        public IItem GetItemByItemNumber(int itemNumber)
        {
            if(_itemDataKeyDict.TryGetValue(itemNumber,out IItem iteminfo))
            {
                return iteminfo;
            }
            return null;
        }
        public IItem GetRandomItem(Type itemtype)
        {
            IItem item = null;
            List<int> keylist;
            int randomKey = 0;

            Dictionary<int, IItem> itemDict = _allItemDataDict[itemtype];
            keylist = itemDict.Keys.ToList();
            randomKey = keylist[Random.Range(0, keylist.Count)];
            item = itemDict[randomKey];

            return item;
        }
        public IItem GetRandomItemFromAll()
        {
            Type randomType = _itemDataType[Random.Range(0, _itemDataType.Count)];
            return GetRandomItem(randomType);
        }
       
        public GameObject GetEquipLootItem(IItem iteminfo)
        {
            GameObject lootItem;
            switch ((iteminfo as ItemEquipment).EquipmentSlotType)
            {
                case EquipmentSlotType.Helmet:
                case EquipmentSlotType.Armor:
                    lootItem = _resourcesServices.InstantiateByKey("Prefabs/NGO/LootingItem/Shield");
                    break;
                case EquipmentSlotType.Weapon:
                    lootItem = _resourcesServices.InstantiateByKey("Prefabs/NGO/LootingItem/Sword");
                    break;
                default:
                    lootItem = _resourcesServices.InstantiateByKey("Prefabs/NGO/LootingItem/Bag");
                    break;
            }
            lootItem.GetComponent<LootItem>().SetIteminfo(iteminfo);
            return lootItem;
        }

        public GameObject GetConsumableLootItem(IItem iteminfo)
        {
            GameObject lootitem = _resourcesServices.InstantiateByKey("Prefabs/NGO/LootingItem/Potion");
            lootitem.GetComponent<LootItem>().SetIteminfo(iteminfo);
            return lootitem;
        }
        
        
        
        private IDictionary BindImageSources(IDictionary missingImageItemsDict)
        {
            IDictionary itemDict;
            itemDict = missingImageItemsDict;

            foreach (System.Object key in itemDict.Keys)
            {
                IItem itemType = itemDict[key] as IItem;
                FindImageByKey(itemType);
            }
            return itemDict;
        }//딕셔너리로 던져줘야함.

        private void FindImageByKey(IItem items)
        {
            items.ImageSource[items.ItemIconSourceText] = _resourcesServices.Load<Sprite>($"Art/UI/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/Icon_EquipIcons/Shadow/256/{items.ItemIconSourceText}");
        }
    }
}
