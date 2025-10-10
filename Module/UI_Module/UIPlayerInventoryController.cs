using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.InputManager;
using GameManagers.Interface.SceneUIManager;
using GameManagers.Interface.UIFactoryManager.SceneUI;
using GameManagers.Interface.UIFactoryManager.UIController;
using GameManagers.Interface.UIManager;
using UI.Popup.PopupUI;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;
using Zenject;

namespace Module.UI_Module
{
    public class UIPlayerInventoryController : MonoBehaviour
    {
        public class UIPlayerInventoryControllerFactory : SceneComponentFactory<UIPlayerInventoryController> { }
        
        private UIPlayerInventory _inventoryUI;
        private InputAction _switchInventoryUI;
        [Inject] private IUIManagerServices _uiManager;
        [Inject] private IInputAsset _inputManager;
        [Inject] private IPlayerSpawnManager _gameManagerEx;
        private void Awake()
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

        private void OnEnable()
        {
            _switchInventoryUI.performed += SwitchInventory;
        }

        private void OnDisable()
        {
            _switchInventoryUI.performed -= SwitchInventory;
        }

        public void SwitchInventory(InputAction.CallbackContext context)
        {
            _uiManager.SwitchPopUpUI(_inventoryUI);
        }
    }
}