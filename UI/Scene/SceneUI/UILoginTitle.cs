using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using UI.Popup.PopupUI;
using UnityEngine.UI;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UILoginTitle : UIScene
    {
        [Inject] private IUIManagerServices _uiManager;
        
        enum ButtonEvent
        {
            ButtonStart,
        }

        private Button _openLoginButton;
        private UILoginPopup _uiLoginPopup;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Button>(typeof(ButtonEvent));
            _openLoginButton = Get<Button>((int)ButtonEvent.ButtonStart);
        }

        protected override void StartInit()
        {
            base.StartInit();
            _openLoginButton.onClick.AddListener(ClickLoginButton);
        }
    

        public void ClickLoginButton()
        {
            if(_uiLoginPopup == null)
            {
                _uiLoginPopup = _uiManager.GetPopupUIFromResource<UILoginPopup>();
            }
            _uiManager.ShowPopupUI(_uiLoginPopup);
        }


    }
}
