using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UI.Popup.PopupUI
{
    public abstract class UIAlertPopupBase : UIPopup
    {
        [Inject] private IUIManagerServices _uiManagerServices;
        enum Texts
        {
            TitleText,
            BodyText,
        }

        enum Buttons
        {
            ConfirmButton
        }
        protected TMP_Text _titleText;
        protected TMP_Text _bodyText;
        protected Button _confirm_Button;

        // [핵심 1] 외부 동작을 저장할 대리자(Delegate) 변수 추가
        private UnityAction _customCloseAction;

        public void SetText(string titleText,string bodyText)
        {
            _titleText.text = titleText;
            _bodyText.text = bodyText;
        }

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<TMP_Text>(typeof(Texts));
            Bind<Button>(typeof(Buttons));
            
            _titleText = Get<TMP_Text>((int)Texts.TitleText);
            _bodyText = Get<TMP_Text>((int)(Texts.BodyText));
            _confirm_Button = Get<Button>((int)Buttons.ConfirmButton);
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            _uiManagerServices.AddImportant_Popup_UI(this);
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _confirm_Button.onClick.RemoveListener(OnClickConfirmButton);
            _confirm_Button.onClick.AddListener(OnClickConfirmButton);
        }

        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            
            _customCloseAction = null; 
        }

        private void OnClickConfirmButton()
        {
            _customCloseAction?.Invoke();
            
            _uiManagerServices.ClosePopupUI(this);
        }

        public void SetCloseButtonOverride(UnityAction closeButtonAction)
        {
            _customCloseAction = closeButtonAction;
        }
    }
}
