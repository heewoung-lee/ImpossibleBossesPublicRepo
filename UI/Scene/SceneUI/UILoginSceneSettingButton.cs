using System;
using GameManagers;
using GameManagers.UIManagement;
using UI.Popup.PopupUI;
using UnityEngine.UI;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UILoginSceneSettingButton : UIScene
    {
        [Inject] private IUIManagerServices _uiManager;

        private enum Buttons
        {
            ButtonSetting
        }

        private UISettingsPopup _settingsPopup;
        private Button _settingButton;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            AddBind<Button>(typeof(Buttons), out string[] _);
            _settingButton = GetButton((int)Buttons.ButtonSetting);
        }

        protected override void StartInit()
        {
            base.StartInit();
            _settingButton.onClick.AddListener(ShowSettingPopupUI);
        }

        private void ShowSettingPopupUI()
        {
            if(_settingsPopup == null)
            {
                _settingsPopup = _uiManager.GetPopupUIFromResource<UISettingsPopup>();
            }
            _uiManager.ShowPopupUI(_settingsPopup);
        }
        
    }
}
