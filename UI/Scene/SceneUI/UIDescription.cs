using System;
using DataType;
using DataType.Item;
using DataType.Item.Consumable;
using DataType.Item.Equipment;
using DataType.Skill;
using GameManagers;
using GameManagers.GameManagerExManagement;
using GameManagers.UIManagement;
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
        private const string DurationLabel = "지속시간 :";
        private const string CooldownLabel = "쿨타임 :";

        private IUIManagerServices _uiManagerServices;
        private IPlayerSpawnManager _gameManagers;

        [Inject]
        public void Construct(IUIManagerServices uiManagerServices, IPlayerSpawnManager gameManagers)
        {
            _uiManagerServices = uiManagerServices;
            _gameManagers = gameManagers;
        }
        
        
        enum ImageType
        {
            IconImage
        }

        enum ItemDescription
        {
            ItemNameText,
            ItemTypeText,
        }

        enum Direction
        {
            Right,
            Down,
            Left,
            Up,
        }

        private Image _iconImage;
        private TMP_Text _itemNameText;
        private TMP_Text _itemTypeText;
        private TMP_Text _itemDescriptionText;
        private DescriptionWindow _descriptionWindow;
        private RectTransform _descriptionRectTr;
        private Canvas _descriptionCanvas;
        private Direction _currntDir = Direction.Right;
        private Vector3 _originPos;
        private UIPlayerInventory _uiPlayerInventory;
        private GameObject _topSection;
        private GameObject _statListRoot;
        private UIStatList _statList;
        private GameObject _bottomSection;
        private GameObject _skillSection;
        private GameObject _durationRow;
        private GameObject _skillBackGround;
        private TMP_Text _durationLabelText;
        private TMP_Text _durationValueText;
        private TMP_Text _skillDescriptionText;
        private bool _hasDescriptionText;
        private bool _hasDurationText;
        public DescriptionWindow DescriptionWindow => _descriptionWindow;

        public UIPlayerInventory UIPlayerInventory
        {
            get
            {
                if (_uiPlayerInventory == null)
                {
                    _uiPlayerInventory = _uiManagerServices.GetImportant_Popup_UI<UIPlayerInventory>();
                }

                return _uiPlayerInventory;
            }
        }

        public Vector3 OriginPos
        {
            get => _originPos;
        }

        public bool IsDescriptionActive => gameObject.activeSelf;

        protected override void AwakeInit()
        {
            Bind<Image>(typeof(ImageType));
            Bind<TMP_Text>(typeof(ItemDescription));

            _iconImage = Get<Image>((int)ImageType.IconImage);
            _itemNameText = Get<TMP_Text>((int)ItemDescription.ItemNameText);
            _itemTypeText = Get<TMP_Text>((int)ItemDescription.ItemTypeText);
            _topSection = Utill.FindChildGameObject(gameObject, "TopSection");
            _statListRoot = Utill.FindChildGameObject(gameObject, "StatList");
            _bottomSection = Utill.FindChildGameObject(gameObject, "BottomSection");
            _skillSection = Utill.FindChildGameObject(gameObject, "SkillSection");
            _skillBackGround = Utill.FindChildGameObject(gameObject, "SkillBackGround");
            _itemDescriptionText = Utill.FindChildComponent<TMP_Text>(_bottomSection, "DescriptionText");
            _skillDescriptionText = Utill.FindChildComponent<TMP_Text>(_skillSection, "DescriptionText");
            _durationRow = Utill.FindChildGameObject(_bottomSection, "DurationRow");
            _durationLabelText = Utill.FindChildComponent<TMP_Text>(_durationRow, "DurationLabelText");
            _durationValueText = Utill.FindChildComponent<TMP_Text>(_durationRow, "DurationValueText");
            _statList = _statListRoot.GetComponent<UIStatList>();
            if (_statList == null)
            {
                _statList = _statListRoot.AddComponent<UIStatList>();
            }

            _statList.Initialize();
            _originPos = transform.position;

            _descriptionWindow = GetComponentInChildren<DescriptionWindow>();
            _descriptionRectTr = _descriptionWindow.GetComponent<RectTransform>();
            _descriptionCanvas = GetComponent<Canvas>();

            SetTopSectionActive(false);
            SetSkillBackGroundActive(false);
            SetSkillSectionActive(false);
            HideStatRows();
            SetDescription("");
            SetDuration(0f);
            gameObject.SetActive(false);
        }

        public void UI_DescriptionEnable() => gameObject.SetActive(true);
        public void UI_DescriptionDisable() => gameObject.SetActive(false);

        public void SetValue(BaseDataSO data)
        {
            if (data == null) return;

            _iconImage.sprite = data.icon;
            _itemNameText.text = data.dataName;
            _itemNameText.color = Color.white;
            _itemTypeText.text = "";
            SetTopSectionActive(false);
            SetSkillBackGroundActive(false);
            SetSkillSectionActive(false);
            HideStatRows();
            SetSkillDescription("");
            SetDescription("");
            SetDuration(0f);

            string mainDescription = "";
            string subDescription = "";

            if (data is ItemDataSO itemData)
            {
                Color itemGradeColor = Utill.GetItemGradeColor(itemData.itemGrade);
                _itemNameText.color = _itemNameText.SetGradeColor(itemGradeColor);
                SetTopSectionActive(true);
                _itemTypeText.text = GetItemType(itemData);

                if (TrySetItemStatRows(itemData))
                {
                    return;
                }

                mainDescription = itemData.GetItemEffectText();

                subDescription = itemData.description;
            }
            else if (data is SkillDataSO skillData)
            {
                SetSkillBackGroundActive(true);
                SetSkillSectionActive(true);
                PlayerStats playerStats = _gameManagers.GetPlayerStats();
                SetSkillDescription(Utill.GetFinalDescription(skillData, playerStats));
                SetCooldown(skillData.cooldown);
                return;
            }
            else
            {
                mainDescription = data.description;
            }

            SetPlainDescription(mainDescription, subDescription);
        }

        public void SetDescription(string text)
        {
            _hasDescriptionText = string.IsNullOrEmpty(text) == false;
            _itemDescriptionText.gameObject.SetActive(_hasDescriptionText);
            _itemDescriptionText.text = text;
            RefreshBottomSection();
        }

        private void SetSkillDescription(string text)
        {
            _skillDescriptionText.text = text;
        }

        private string GetItemType(ItemDataSO data)
        {
            switch (data.ItemType)
            {
                case ItemType.Equipment:
                    if (data is EquipmentItemSO equipData)
                    {
                        return ConvertEquipItemTypeToKorean(equipData.slotType);
                    }

                    return "알수없는장비";
                case ItemType.Consumable:
                    return "소비아이템";
                case ItemType.ETC:
                    return "기타아이템";
            }

            return "기타아이템";
        }

        private string ConvertEquipItemTypeToKorean(EquipmentSlotType equipType)
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

        private void SetPlainDescription(string mainDescription, string subDescription)
        {
            if (string.IsNullOrEmpty(subDescription))
            {
                SetDescription(mainDescription);
                return;
            }

            if (string.IsNullOrEmpty(mainDescription))
            {
                SetDescription(subDescription);
                return;
            }

            SetDescription($"{mainDescription}\n{subDescription}");
        }

        private bool TrySetItemStatRows(ItemDataSO itemData)
        {
            if (itemData is ExperienceBookItemSO experienceBookData)
            {
                _statList.SetDisplayRow(null, $"경험치 +{FormatNumber(experienceBookData.experienceAmount)}");
                SetDescription(experienceBookData.description);
                return true;
            }

            if (itemData is ConsumableItemSO consumableData)
            {
                _statList.SetStats(consumableData.itemEffects);
                SetDescription(consumableData.description);
                SetDuration(consumableData.duration);
                return true;
            }

            if (itemData is EquipmentItemSO equipData)
            {
                _statList.SetStats(equipData.itemEffects);
                SetDescription(equipData.description);
                return true;
            }

            return false;
        }

        private void HideStatRows()
        {
            _statList.Hide();
        }

        private void SetTopSectionActive(bool isActive)
        {
            _topSection.SetActive(isActive);
        }

        private void SetSkillBackGroundActive(bool isActive)
        {
            _skillBackGround.SetActive(isActive);
        }

        private void SetSkillSectionActive(bool isActive)
        {
            _skillSection.SetActive(isActive);
        }

        private string GetDurationText(float duration)
        {
            return $"{FormatNumber(duration)}초";
        }

        private void SetDuration(float duration)
        {
            SetBottomTimeRow(DurationLabel, duration);
        }

        private void SetCooldown(float cooldown)
        {
            SetBottomTimeRow(CooldownLabel, cooldown);
        }

        private void SetBottomTimeRow(string labelText, float duration)
        {
            _hasDurationText = duration > 0f;
            _durationRow.SetActive(_hasDurationText);

            if (_hasDurationText == false)
            {
                _durationValueText.text = "";
                RefreshBottomSection();
                return;
            }

            _durationLabelText.text = labelText;
            _durationValueText.text = GetDurationText(duration);
            RefreshBottomSection();
        }

        private void RefreshBottomSection()
        {
            _bottomSection.SetActive(_hasDescriptionText || _hasDurationText);
        }

        private string FormatNumber(float value)
        {
            float roundedValue = Mathf.Round(value);
            if (Mathf.Approximately(value, roundedValue))
            {
                return roundedValue.ToString("0");
            }

            return value.ToString("0.##");
        }

        protected override void StartInit()
        {
            _descriptionCanvas.sortingOrder = (int)Define.SpecialSortingOrder.Description;
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            _statList.CacheStatIcons(_resourcesServices);
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _statList.CacheStatIcons(_resourcesServices);
            if (UIPlayerInventory == null) return;
            _descriptionCanvas.sortingOrder = UIPlayerInventory.GetComponent<Canvas>().sortingOrder + 1;
        }

        //위치 계산 로직 (화면 안 나가게 조정)
        //2.25일 수정 모든 연산을 실제 화면 픽셀 기준으로 통일해서 계산
        //이전에는 논리적크기를 픽셀좌표에 더해 오차가 발생한걸
        //lossyScale을 곱해 실제 픽셀로 변환한뒤 경계를 검사해 보다 깔끔하고 다 해상도를 지원하게 만듦
        public Vector3 SetDecriptionPos(Transform componentTr, float width, float height)
        {
            // 변경점 1: 타겟의 시작 위치를 스크린 픽셀 좌표로 고정
            // 이전: 자기 자신(_descriptionRectTr)의 로컬 좌표계로 변환하여 기준점이 계속 흔들렸음.
            // 현재(2.25): 스킬 아이콘(componentTr)의 월드 좌표를 기준이 되는 스크린 픽셀 좌표로 변환하여 고정.
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, componentTr.position);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // 변경점 2: 논리적 크기(Rect)를 실제 기기의 물리적 픽셀 크기로 변환
            // 이전: 스케일 보정 없이 _descriptionRectTr.rect.width를 그대로 사용. 이러면 다 해상도 지원 불가
            // 현재(2.25): lossyScale(월드 스케일)을 곱하여, 현재 화면에서 이 UI가 차지하는 '진짜 픽셀 크기'를 구함.
            Vector2 tooltipScreenSize = new Vector2(
                _descriptionRectTr.rect.width * _descriptionRectTr.lossyScale.x,
                _descriptionRectTr.rect.height * _descriptionRectTr.lossyScale.y
            );

            Vector2 componentScreenSize = new Vector2(
                width * componentTr.lossyScale.x,
                height * componentTr.lossyScale.y
            );

            _currntDir = Direction.Right;
            int directionSettingCount = 0;

            while (directionSettingCount < 4)
            {
                Vector2 targetScreenPos = screenPoint;

                // 변경점 3: 피벗 변경 및 스크린 픽셀 기준 오프셋 적용
                // 이전: 로컬 좌표에서 오프셋을 더한 뒤 TransformPoint로 다시 월드 좌표로 변환하는 과정으로 수행했지만,
                // 현재: 스크린 픽셀 좌표(targetScreenPos)에 아이콘의 픽셀 절반 크기를 더하고 빼서 위치 설정.
                switch (_currntDir)
                {
                    case Direction.Right:
                        _descriptionRectTr.pivot = new Vector2(0, 1);
                        targetScreenPos.x += componentScreenSize.x / 2;
                        targetScreenPos.y += componentScreenSize.y / 2;
                        break;
                    case Direction.Down:
                        _descriptionRectTr.pivot = new Vector2(0.5f, 1);
                        targetScreenPos.y -= componentScreenSize.y / 2;
                        break;
                    case Direction.Left:
                        _descriptionRectTr.pivot = new Vector2(1, 1);
                        targetScreenPos.x -= componentScreenSize.x / 2;
                        targetScreenPos.y += componentScreenSize.y / 2;
                        break;
                    case Direction.Up:
                        _descriptionRectTr.pivot = new Vector2(0.5f, 0);
                        targetScreenPos.y += componentScreenSize.y / 2;
                        break;
                }

                //결정된 위치가 화면(Screen) 밖으로 나가는지 경계 검사
                float minX = targetScreenPos.x - (_descriptionRectTr.pivot.x * tooltipScreenSize.x);
                float maxX = targetScreenPos.x + ((1 - _descriptionRectTr.pivot.x) * tooltipScreenSize.x);
                float minY = targetScreenPos.y - (_descriptionRectTr.pivot.y * tooltipScreenSize.y);
                float maxY = targetScreenPos.y + ((1 - _descriptionRectTr.pivot.y) * tooltipScreenSize.y);

                // 화면 안에 완전히 들어오는 경우
                if (minX >= 0 && maxX <= screenWidth && minY >= 0 && maxY <= screenHeight)
                {
                    // 변경점 4: 최종 반환 시 기준을 '부모 캔버스'로 설정
                    // 이전: 툴팁 자신의 공간(TransformPoint)을 참조하여 오작동 발생.(내가 왜 그랬지? 아마 문제가 없어서 넘긴것 같다)
                    // 현재: 계산이 끝난 완벽한 스크린 좌표(targetScreenPos)를, 툴팁이 실제로 붙어있는 '부모 캔버스'의 월드 좌표로 변환.
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        _descriptionRectTr.parent as RectTransform,
                        targetScreenPos,
                        null,
                        out Vector3 finalWorldPos
                    );
                    return finalWorldPos;
                }

                // 화면을 벗어나면 다음 방향으로 전환
                _currntDir = (Direction)(((int)_currntDir + 1) % Enum.GetValues(typeof(Direction)).Length);
                directionSettingCount++;
            }

            // 4방향 모두 화면을 벗어날 경우 기본 피벗 및 원위치 리턴
            _descriptionRectTr.pivot = new Vector2(0.5f, 0.5f);
            return OriginPos;
        }

        public void SetdecriptionOriginPos()
        {
            transform.position = OriginPos;
        }
    }
}
