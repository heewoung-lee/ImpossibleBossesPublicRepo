using CoreScripts;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.InputManager;
using GameManagers.UIFactory.UIController;
using UI.Popup.PopupUI;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;
using Zenject;

namespace Module.UI_Module
{
    public class UIPlayerInventoryController : ZenjectMonoBehaviour
    {
        public class UIPlayerInventoryControllerFactory : SceneComponentFactory<UIPlayerInventoryController> { }
        
        private UIPlayerInventory _inventoryUI;
        private InputAction _switchInventoryUI;
        [Inject] private IUIManagerServices _uiManager;
        [Inject] private IInputAsset _inputManager;
        [Inject] private IPlayerSpawnManager _gameManagerEx;
        protected override void ZenjectEnable()
        {
            _switchInventoryUI.performed += SwitchInventory;
        }
        protected override void ZenjectDisable()
        {
            _switchInventoryUI.performed -= SwitchInventory;
        }
        protected override void InitAfterInject()
        {
            _switchInventoryUI = _inputManager.GetInputAction(Define.ControllerType.UI, "Show_UI_Inventory");
            _switchInventoryUI.Enable();
            if (_gameManagerEx.GetPlayer() == null)
            {
                _gameManagerEx.OnPlayerSpawnEvent += (playerStats) => InitializeInventor();
            }
            else
            {
                InitializeInventor();
            }
            void InitializeInventor()
            {
                _inventoryUI = _uiManager.GetPopupUIFromResource<UIPlayerInventory>();
            }
        }

        public void SwitchInventory(InputAction.CallbackContext context)
        {
            _uiManager.SwitchPopUpUI(_inventoryUI);
        }

    }
}