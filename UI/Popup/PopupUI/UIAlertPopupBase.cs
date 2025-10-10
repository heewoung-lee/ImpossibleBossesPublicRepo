using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using TMPro;
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
            _uiManagerServices.AddImportant_Popup_UI(this);
        }


        protected override void OnEnableInit()
        {
            base.OnEnableInit();
            _confirm_Button.onClick.AddListener(() => { _uiManagerServices.ClosePopupUI(this); });
        }

        protected override void OnDisableInit()
        {
            base.OnDisableInit();
            _confirm_Button.onClick.RemoveAllListeners();
        }

        public void SetCloseButtonOverride(UnityAction closeButtonAction)
        {
            _confirm_Button.onClick.AddListener(closeButtonAction);
        }

    }
}
