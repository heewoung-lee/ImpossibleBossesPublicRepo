using System;
using GameManagers;
using GameManagers.Interface.BufferManager;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.InputManager;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.SceneUIManager;
using GameManagers.Interface.UIFactoryManager.SceneUI;
using GameManagers.Interface.UIManager;
using Stats;
using UI.SubItem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UIConsumableBar : UIScene
    {
        public class UIConsumableBarFactory : SceneUIFactory<UIConsumableBar>{}
        
        [Inject] private IInputAsset _inputManager;
        [Inject] private IPlayerSpawnManager _gameManagerEx;
        [Inject] private IUIManagerServices _uiManagerServices; 
        [Inject] private IBufferManager _bufferManager;


        public void Construct(
            IInputAsset inputmanager, 
            IPlayerSpawnManager gameManagerEx, 
            IUIManagerServices uiManagerServices, 
            IBufferManager bufferManager)
        {
            _inputManager = inputmanager;
            _gameManagerEx = gameManagerEx;
            _uiManagerServices = uiManagerServices;
            _bufferManager = bufferManager;
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
                if(_itemDragImage == null)
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
                if(_uiDescription == null)
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

        protected override void AwakeInit()
        {
            base.AwakeInit();
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
                _comsumableGetKey[i] = _inputManager.GetInputAction(Define.ControllerType.UI, $"Consumabar_GetKey{i + 1}");
                _comsumableGetKey[i].Enable();
            }
        }


        public void UsedPosition(InputAction.CallbackContext context)
        {
            int inputKey = int.Parse(context.control.path.Replace("/Keyboard/",""))-1;

            if (_frameTrs[inputKey].gameObject.TryGetComponentInChildren(out UIItemComponentConsumable consumable))
            {
                if (consumable.ItemCount > 1)
                {
                    consumable.ItemCount--;
                }
                else
                {
                    _resourcesServices.DestroyObject(consumable.gameObject);
                }
                foreach (StatEffect effect in consumable.ItemEffects)
                {
                    _bufferManager.InitBuff(_playerStats,consumable.DuringBuffTime,effect);
                }
            }

            if (ItemDragImage.IsDragImageActive == true)
            {
                ItemDragImage.SetItemImageDisable();
            }
            if(UIDescription.IsDescriptionActive == true)
            {
                UIDescription.UI_DescriptionDisable();
            }

        }


        protected override void StartInit()
        {
        
        }

        private void OnEnable()
        {
            if (_gameManagerEx.GetPlayer() == null)
            {
                _gameManagerEx.OnPlayerSpawnEvent += SetPlayerComsumableBarUI;
            }
            else
            {
                SetPlayerComsumableBarUI(_gameManagerEx.GetPlayer().GetComponent<PlayerStats>());
            }
        }

        private void OnDisable()
        {
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
    }
}