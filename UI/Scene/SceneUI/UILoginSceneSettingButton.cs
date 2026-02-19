using System;
using GameManagers;
using UI.Popup.PopupUI;
using UnityEngine.UI;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UILoginSceneSettingButton : UIScene
    {
        [Inject] private IUIManagerServices _uiManager;
        
        private UISettingsPopup _settingsPopup;
        private Button _settingButton; 
        
        private void Awake()
        {
            _settingButton = GetComponent<Button>();
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
