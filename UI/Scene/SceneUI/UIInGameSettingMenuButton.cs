using GameManagers;
using GameManagers.UIFactoryManagement.SceneUI;
using GameManagers.UIManagement;
using UI.Popup.PopupUI;
using UnityEngine.UI;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UIInGameSettingMenuButton : UIScene
    {
        private IUIManagerServices _uiManagerServices;   
        
        [Inject]
        public void Construct(IUIManagerServices uiManagerServices)
        {
            _uiManagerServices = uiManagerServices;
        }
        
        public class UIInGameSettingMenuFactory : SceneUIFactory<UIInGameSettingMenuButton>{}
        
        enum Buttons
        {
            SettingButton
        }

        private Button _settingButton;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Button>(typeof(Buttons));
            _settingButton = Get<Button>((int)Buttons.SettingButton);
            
            _settingButton.onClick.AddListener(ShowSettingUI);
        }

        private void ShowSettingUI()
        {
            if (_uiManagerServices.TryGetPopupDictAndShowPopup(out UIPlaySceneEscMenuPopup settingsPopup))
            {
                _uiManagerServices.ShowPopupUI(settingsPopup);
            }
        }
        
        
        
    }
}
