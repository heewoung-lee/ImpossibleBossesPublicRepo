using System;
using Data.DataType.ItemType.Interface;
using DataType; // ItemType
using DataType.Item; // ItemDataSO
using DataType.Item.Equipment;
using DataType.Skill;
using GameManagers;
using GameManagers.Interface.GameManagerEx; // EquipmentItemSO, EquipmentSlotType
using GameManagers.Interface.UIManager;
using Skill;
using Stats;
using TMPro;
using UI.Popup.PopupUI;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UIDescription : UIScene
    {
        [Inject] private IUIManagerServices _uiManagerServices;
        [Inject] private IPlayerSpawnManager _gameManagers;
        enum ImageType
        {
            ItemImage
        }

        enum ItemDescription
        {
            ItemNameText,
            ItemType,
            ItemGradeType,
            ItemEffectText,
            DescriptionText,
        }

        enum Direction
        {
            Right,
            Down,
            Left,
            Up,
        }

        private Image _itemImage;
        private TMP_Text _itemNameText;
        private TMP_Text _itemType;
        private TMP_Text _itemEffectText;
        private TMP_Text _itemDescriptionText;
        private DescriptionWindow _descriptionWindow;
        private RectTransform _descriptionRectTr;
        private float _descriptionWidth;
        private float _descriptionHeight;
        private Canvas _descriptionCanvas;
        private Direction _currntDir = Direction.Right;
        private Color _itemGradeColor;
        private Vector3 _originPos;
        private UIPlayerInventory _uiPlayerInventory;

        public DescriptionWindow DescriptionWindow => _descriptionWindow;

        public UIPlayerInventory UIPlayerInventory
        {
            get
            {
                if(_uiPlayerInventory == null)
                {
                    _uiPlayerInventory = _uiManagerServices.GetImportant_Popup_UI<UIPlayerInventory>();
                }
                return _uiPlayerInventory;
            }
        }

        public Vector3 OriginPos { get => _originPos; }
        public bool IsDescriptionActive => gameObject.activeSelf;

        protected override void AwakeInit()
        {
            Bind<Image>(typeof(ImageType));
            Bind<TMP_Text>(typeof(ItemDescription));

            _itemImage = Get<Image>((int)ImageType.ItemImage);
            _itemNameText = Get<TMP_Text>((int)ItemDescription.ItemNameText);
            _itemType = Get<TMP_Text>((int)ItemDescription.ItemType);
            _itemEffectText = Get<TMP_Text>((int)ItemDescription.ItemEffectText);
            _itemDescriptionText = Get<TMP_Text>((int)ItemDescription.DescriptionText);
            _originPos = transform.position;

            _descriptionWindow = GetComponentInChildren<DescriptionWindow>();
            _descriptionRectTr = _descriptionWindow.GetComponent<RectTransform>();
            _descriptionWidth = _descriptionRectTr.rect.width;
            _descriptionHeight = _descriptionRectTr.rect.height;
            _descriptionCanvas = GetComponent<Canvas>();

            gameObject.SetActive(false);
        }

        public void UI_DescriptionEnable() => gameObject.SetActive(true);
        public void UI_DescriptionDisable() => gameObject.SetActive(false);

        public void SetValue(BaseDataSO data)
        {
            if (data == null) return;

            _itemImage.sprite = data.icon;
            _itemNameText.text = data.dataName;
            _itemNameText.color = Color.white; 
            _itemType.text = ""; 

            string mainDescription = ""; 
            string subDescription = "";  

            if (data is ItemDataSO itemData)
            {
        
                _itemGradeColor = Utill.GetItemGradeColor(itemData.itemGrade);
                _itemNameText.color = _itemNameText.SetGradeColor(_itemGradeColor);
                _itemType.text = GetItemType(itemData);

                mainDescription = itemData.GetItemEffectText(); 
        
                subDescription = itemData.description; 
            }
            else if (data is SkillDataSO skillData)
            {
                PlayerStats playerStats = _gameManagers.GetPlayerStats();
                mainDescription = Utill.GetFinalDescription(skillData, playerStats);
                subDescription = skillData.etcDescription;
            }
            else
            {
                mainDescription = data.description;
            }

            SetItemEffectText(mainDescription); // 중앙
            SetDescription(subDescription);     // 하단
        }

        public void SetItemEffectText(string text)
        {
            _itemEffectText.text = text;
        }

        public void SetDescription(string text)
        {
            _itemDescriptionText.text = text;
        }

        // [변경] IItem -> ItemDataSO (장비 캐스팅 변경)
        public string GetItemType(ItemDataSO data)
        {
            switch (data.ItemType)
            {
                case ItemType.Equipment:
                    if (data is EquipmentItemSO equipData)
                    {
                        return ConvertEquipItemTypeToKorean(equipData.slotType);
                    }
                    return "장비"; // 예외 처리
                case ItemType.Consumable:
                    return "소비아이템";
                case ItemType.ETC:
                    return "기타아이템";
            }
            return "기타아이템";
        }

        // [변경] EquipmentSlotType 경로 명확히
        public string ConvertEquipItemTypeToKorean(EquipmentSlotType equipType)
        {
            switch (equipType)
            {
                case EquipmentSlotType.Helmet: return "머리";
                case EquipmentSlotType.Gauntlet: return "장갑";
                case EquipmentSlotType.Shoes: return "신발";
                case EquipmentSlotType.Weapon: return "무기";
                case EquipmentSlotType.Ring: return "반지";
                case EquipmentSlotType.Armor: return "갑옷";
            }
            return "알수없는장비";
        }

        protected override void StartInit()
        {
            _descriptionCanvas.sortingOrder = (int)Define.SpecialSortingOrder.Description;
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            if (UIPlayerInventory == null) return;
            _descriptionCanvas.sortingOrder = UIPlayerInventory.GetComponent<Canvas>().sortingOrder + 1;
        }

        // [기존 유지] 위치 계산 로직 (화면 안 나가게 조정)
        public Vector3 SetDecriptionPos(Transform componentTr, float width, float height)
        {
            _currntDir = Direction.Right;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, componentTr.position);
            Vector2 localPoint;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _descriptionRectTr,
                screenPoint,
                null,
                out localPoint
            );

            Vector3 setPos = localPoint;
            int directionSettingCount = 0;

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            while (true)
            {
                directionSettingCount++;
                if (directionSettingCount > 4)
                {
                    _descriptionRectTr.pivot = new Vector2(0.5f, 0.5f);
                    return OriginPos;
                }

                switch (_currntDir)
                {
                    case Direction.Right:
                        _descriptionRectTr.pivot = new Vector2(0, 1);
                        setPos = new Vector2(localPoint.x + width / 2, localPoint.y + height / 2);
                        break;
                    case Direction.Down:
                        _descriptionRectTr.pivot = new Vector2(0.5f, 1);
                        setPos = new Vector2(localPoint.x + width / 2, localPoint.y - height / 2);
                        break;
                    case Direction.Left:
                        _descriptionRectTr.pivot = new Vector2(1, 1);
                        setPos = new Vector2(localPoint.x - width / 2, localPoint.y + height / 2);
                        break;
                    case Direction.Up:
                        _descriptionRectTr.pivot = new Vector2(0.5f, 0);
                        setPos = new Vector2(localPoint.x + width / 2, localPoint.y + height / 2);
                        break;
                }

                Vector3 worldPos = _descriptionRectTr.TransformPoint(setPos);
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);

                float minPositionX = screenPos.x + (_descriptionRectTr.pivot.x * -1 * _descriptionRectTr.rect.width);
                float maxPositionX = screenPos.x + (_descriptionRectTr.pivot.x * -1 + 1) * _descriptionRectTr.rect.width;
                float minPositionY = screenPos.y + (_descriptionRectTr.pivot.y * -1 * _descriptionRectTr.rect.height);
                float maxPositionY = screenPos.y + (_descriptionRectTr.pivot.y * -1 + 1) * _descriptionRectTr.rect.height;

                if (minPositionX > 0 && maxPositionX < screenWidth && minPositionY > 0 && minPositionY < screenHeight)
                {
                    return screenPos;
                }
                else
                {
                    _currntDir = (Direction)(((int)_currntDir + 1) % Enum.GetValues(typeof(Direction)).Length);
                    continue;
                }
            }
        }
        
        public void SetdecriptionOriginPos()
        {
            transform.position = OriginPos;
        }
    }
}