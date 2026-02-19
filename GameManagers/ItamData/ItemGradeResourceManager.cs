using System.Collections.Generic;
using System.Threading.Tasks;
using Data.DataType.ItemType.Interface;
using DataType;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ItamDataManager.Interface;
using GameManagers.ResourcesEx;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers
{
    public class ItemGradeResourceManager : IItemGradeBorder
    {
        private IResourcesServices _resourcesServices;
        private const string ItemFrameBorderPath = "Art/Item/UIItem/ItemBorderImages";
        private Dictionary<ItemGradeType, Sprite> _itemGradeBorder;
        [Inject]
        public ItemGradeResourceManager(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
            Initialize();
        }
        public Sprite GetGradeBorder(ItemGradeType gradeType)
        {
            if (_itemGradeBorder != null && _itemGradeBorder.TryGetValue(gradeType, out var sprite))
                return sprite;

            UtilDebug.LogWarning($"[GetGradeBorder] 해당 gradeType({gradeType})이 존재하지 않습니다.");
            return null;
        }
        public void Initialize()
        {
            _itemGradeBorder = new Dictionary<ItemGradeType, Sprite>//아이템 등급 프레임 초기화
            {
                { ItemGradeType.Normal, _resourcesServices.Load<Sprite>(ItemFrameBorderPath + "/Normal") },
                { ItemGradeType.Magic, _resourcesServices.Load<Sprite>(ItemFrameBorderPath + "/Magic") },
                { ItemGradeType.Rare, _resourcesServices.Load<Sprite>(ItemFrameBorderPath + "/Rare") },
                { ItemGradeType.Unique, _resourcesServices.Load<Sprite>(ItemFrameBorderPath + "/Unique") },
                { ItemGradeType.Epic, _resourcesServices.Load<Sprite>(ItemFrameBorderPath + "/Epic") }
            };
        }
    }
}