using GameManagers.InputManagement;
using GameManagers.UIManagement;
using UI;
using UI.Popup.PopupUI;
using UnityEngine.InputSystem;
using Util;
using Zenject;

namespace ScenesScripts
{
    public class UIEscSetting : UIBase
    {
       private IUIManagerServices _uiManagerServices;
       private IInputAsset _inputManager;

        private InputAction _closePopupAction;
        private UILobbyEscMenuPopup _escMenuPopup;
        private bool _escGuard;

        [Inject]
        public void Construct(IUIManagerServices ui, IInputAsset input)
        {
            _uiManagerServices = ui;
            _inputManager = input;
        }


        protected override void StartInit()
        {
            _closePopupAction = _inputManager.GetInputAction(Define.ControllerType.UI, "Close_Popup_UI");
            _closePopupAction.Enable();
            _closePopupAction.performed += OnEscPerformed;
        }

        protected override void AwakeInit()
        {
        
        }

        private void OnDestroy()
        {
            if (_closePopupAction != null)
                _closePopupAction.performed -= OnEscPerformed;
        }

        private void OnEscPerformed(InputAction.CallbackContext ctx)
        {
            
            if (_escGuard) return;
            _escGuard = true;
            try
            {
                if (_uiManagerServices.IsAnyPopupOpen() == true) return;

                
                if (_uiManagerServices.TryGetPopupDictAndShowPopup(out UILobbyEscMenuPopup settingsPopup))
                {
                    _uiManagerServices.ShowPopupUI(settingsPopup);
                }
                
            }
            finally
            {
                _escGuard = false;
            }
        }
    }
}