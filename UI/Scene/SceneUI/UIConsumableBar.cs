using System;
using System.Collections.Generic;
using Controller;
using DataType;
using DataType.Item;
using DataType.Skill;
using DataType.Skill.Factory;
using DataType.Skill.Factory.Effect;
using DataType.Strategies;
using GameManagers;
using GameManagers.Interface.BufferManager;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.InputManager;
using GameManagers.ItamData.Interface;
using GameManagers.Scene;
using GameManagers.UIFactory.SceneUI;
using GameManagers.UIFactory.SubItemUI;
using Skill;
using Stats;
using UI.SubItem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Util;
using Zenject;
using IEffectStrategy = DataType.Skill.Factory.Effect.IEffectStrategy;

namespace UI.Scene.SceneUI
{
    public class UIConsumableBar : UIScene
    {
        public class UIConsumableBarFactory : SceneUIFactory<UIConsumableBar>
        {
        }

        private IInputAsset _inputManager;
        private IPlayerSpawnManager _gameManagerEx;
        private IUIManagerServices _uiManagerServices;
        private IItemDataManager _itemDataManager;
        private IUIItemFactory _itemFactory;
        private IStrategyFactory _strategyFactory;
        private SceneDataSaveAndLoader _saveAndLoader;


        [Inject]
        public void Construct(
            IInputAsset inputmanager,
            IPlayerSpawnManager gameManagerEx,
            IUIManagerServices uiManagerServices,
            IItemDataManager itemDataManager,
            IUIItemFactory itemFactory,
            IStrategyFactory effectFactory,
            SceneDataSaveAndLoader saveAndLoader)
        {
            _inputManager = inputmanager;
            _gameManagerEx = gameManagerEx;
            _uiManagerServices = uiManagerServices;
            _itemDataManager = itemDataManager;
            _itemFactory = itemFactory;
            _saveAndLoader = saveAndLoader;
            _strategyFactory = effectFactory;
        }

        private Image[] _consumableIcons;
        private Transform[] _frameTrs;

        public Transform[] FrameTrs => _frameTrs;

        private InputAction[] _comsumableGetKey;

        private UIBufferBar _uiBufferBar;

        private PlayerStats _playerStats;

        private UIItemDragImage _itemDragImage;

        public UIItemDragImage ItemDragImage
        {
            get
            {
                if (_itemDragImage == null)
                {
                    _itemDragImage = _uiManagerServices.Get_Scene_UI<UIItemDragImage>();
                }

                return _itemDragImage;
            }
        }

        private UIDescription _uiDescription;


        public UIDescription UIDescription
        {
            get
            {
                if (_uiDescription == null)
                {
                    _uiDescription = _uiManagerServices.Get_Scene_UI<UIDescription>();
                }

                return _uiDescription;
            }
        }


        enum ConsumableIcons
        {
            Consumable1icon,
            Consumable2icon,
            Consumable3icon,
            Consumable4icon,
        }

        enum FrameCoordinate
        {
            ContextFrame1,
            ContextFrame2,
            ContextFrame3,
            ContextFrame4
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            InitalizeConsumable();
        }

        private void InitalizeConsumable()
        {
            _consumableIcons = new Image[Enum.GetValues(typeof(ConsumableIcons)).Length];
            Bind<Image>(typeof(ConsumableIcons));
            ConsumableIcons[] consumableIcons = (ConsumableIcons[])System.Enum.GetValues(typeof(ConsumableIcons));
            for (int i = 0; i < _consumableIcons.Length; i++)
            {
                _consumableIcons[i] = Get<Image>((int)consumableIcons[i]);
            }

            _frameTrs = new Transform[Enum.GetValues(typeof(FrameCoordinate)).Length];
            Bind<Transform>(typeof(FrameCoordinate));
            FrameCoordinate[] frameCoordinates = (FrameCoordinate[])System.Enum.GetValues(typeof(FrameCoordinate));
            for (int i = 0; i < _frameTrs.Length; i++)
            {
                _frameTrs[i] = Get<Transform>((int)frameCoordinates[i]);
            }

            _comsumableGetKey = new InputAction[_frameTrs.Length];
            for (int i = 0; i < _comsumableGetKey.Length; i++)
            {
                _comsumableGetKey[i] =
                    _inputManager.GetInputAction(Define.ControllerType.UI, $"Consumabar_GetKey{i + 1}");
                _comsumableGetKey[i].Enable();
            }
        }


        public void UsedPosition(InputAction.CallbackContext context)
        {
            int inputKey = int.Parse(context.control.path.Replace("/Keyboard/", "")) - 1;

            if (_frameTrs[inputKey].gameObject.TryGetComponentInChildren(out UIItemComponentConsumable consumable))
            {
                // 데이터 매니저를 통해 원본 데이터(SO) 조회
                if (_itemDataManager.TryGetItemData(consumable.ItemNumber, out ItemDataSO data))
                {
                    //팩토리에게 전략 요청 (CreateSkill이 아니라 GetStrategy 사용)
                    if (_strategyFactory.GetStrategy(data) is ConsumableStrategy strategy)
                    {
                        // 주인이 쓴게 필요해서 BaseController가 필요하므로 가져옴
                        BaseController controller = _playerStats.GetComponent<BaseController>();
                        if (controller != null)
                        {
                            ItemExecutionContext executionContext = new ItemExecutionContext(controller,data);
                            strategy.Execute(executionContext);
                          
                        }
                    }
                }

                // 아이템 사용 후 개수 감소 또는 파괴
                if (consumable.ItemCount > 1)
                {
                    consumable.ItemCount--;
                }
                else
                {
                    _resourcesServices.DestroyObject(consumable.gameObject);
                }
            }

            // 드래그 중이거나 설명창이 떠있으면 닫기
            if (ItemDragImage.IsDragImageActive == true)
            {
                ItemDragImage.SetItemImageDisable();
            }

            if (UIDescription.IsDescriptionActive == true)
            {
                UIDescription.UI_DescriptionDisable();
            }
        }

        protected override void StartInit()
        {
            if (_saveAndLoader.TryGetLoadConsumableItem(out List<(int count, IteminfoStruct iteminfo)> saveList) ==
                true)
            {
                for (int i = 0; i < saveList.Count; i++)
                {
                    int itemCount = saveList[i].count;
                    IteminfoStruct iteminfo = saveList[i].iteminfo;
                    Transform cunsumableTr = _frameTrs[i];

                    if (_itemDataManager.TryGetItemData(iteminfo.ItemNumber, out ItemDataSO data))
                    {
                        var createdItem = _itemFactory.CreateItemUI(data, cunsumableTr, itemCount);
                        if (createdItem is UIItemComponentConsumable consumableItem)
                        {
                            consumableItem.LoadToSlot(cunsumableTr);
                        }
                    }
                }
            }
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            if (_gameManagerEx.GetPlayer() == null)
            {
                _gameManagerEx.OnPlayerSpawnEvent += SetPlayerComsumableBarUI;
            }
            else
            {
                SetPlayerComsumableBarUI(_gameManagerEx.GetPlayer().GetComponent<PlayerStats>());
            }
        }


        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            foreach (InputAction getKeyEvent in _comsumableGetKey)
            {
                getKeyEvent.performed -= UsedPosition;
                getKeyEvent.Disable();
            }
        }

        private void SetPlayerComsumableBarUI(PlayerStats stats)
        {
            _playerStats = stats;
            foreach (InputAction getKeyEvent in _comsumableGetKey)
            {
                getKeyEvent.performed += UsedPosition;
                getKeyEvent.Enable();
            }
        }

        //여기에 파괴 로직이 들어오면 세이브 데이터로 모든 데이터를 업로드하고,
        //Start가 될때마다 SaveLoader로직에 소비 아이템 슬롯에 이미 있었는지 확인. 

        private void OnDestroy()
        {
            List<(int count, IteminfoStruct iteminfo)> _consumableItemList = new List<(int, IteminfoStruct)>();

            foreach (Transform uiconsumableComponent in _frameTrs)
            {
                UIItemComponentConsumable consumable =
                    uiconsumableComponent.GetComponentInChildren<UIItemComponentConsumable>();
                if (consumable != null)
                {
                    int itemNumber = consumable.ItemNumber;
                    IteminfoStruct itemInfo = new IteminfoStruct(itemNumber);
                    _consumableItemList.Add((consumable.ItemCount, itemInfo));
                }
            }

            _saveAndLoader.SaveConsumableItem(_consumableItemList);
        }
    }
}