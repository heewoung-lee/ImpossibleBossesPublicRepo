using System;
using GameManagers;
using GameManagers.InputManagement;
using GameManagers.SoundManagement;
using GameManagers.UIManagement;
using UnityEngine.InputSystem;
using Util;
using Zenject;

namespace UI.Popup
{
    public abstract class UIPopup : UIBase
    {
        protected InputAction _closePopupUI;
        [Inject] private IUIManagerServices _uiManagerServices;
        [Inject] private IInputAsset _inputManager;

        protected override void AwakeInit() {} // Don't use injected Object

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            _closePopupUI = _inputManager.GetInputAction(Define.ControllerType.UI, "Close_Popup_UI");
            _closePopupUI.Enable();
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _closePopupUI.performed += ClosePopupUI;
        }

        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            _closePopupUI.performed -= ClosePopupUI;
        }

        public void ClosePopupUI(InputAction.CallbackContext context)
        {
            if (_uiManagerServices.IsTopPopupUI(this))
            {
                _soundManagerServices.PlayUiSfx(gameObject, UICommonSoundCueId.Close);
                _uiManagerServices.ClosePopupUI();
            }
        }
    }
}
