using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.LoginManager;
using GameManagers.Interface.UIManager;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UISignUpPopup : IDPwPopup, IUIHasCloseButton
    {
        
        [Inject] private IUIManagerServices _uiManagerServices;
        [Inject] private IWriteGoogleSheet _writeGoogleSheet;
        
        Button _buttonClose;
        Button _buttonSignup;
        TMP_InputField _idInputField;
        TMP_InputField _pwInputField;
        private UIAlertPopupBase _alertPopup;
        private UIAlertPopupBase _confirmPopup;
        
        
        
        public override TMP_InputField IdInputField => _idInputField;

        public override TMP_InputField PwInputField => _pwInputField;

        public Button CloseButton => _buttonClose;

        enum Buttons
        {
            ButtonClose,
            ButtonSignup
        }
        enum InputFields
        {
            IDInputField,
            PwInputField
        }

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<TMP_InputField>(typeof(InputFields));
            Bind<Button>(typeof(Buttons));
            _idInputField = Get<TMP_InputField>((int)InputFields.IDInputField);
            _pwInputField = Get<TMP_InputField>((int)InputFields.PwInputField);
            _buttonClose = Get<Button>((int)Buttons.ButtonClose);
            _buttonSignup = Get<Button>((int)Buttons.ButtonSignup);
            _buttonClose.onClick.AddListener(OnClickCloseButton);
            _buttonSignup.onClick.AddListener(CreateID);
        }
        public async void CreateID()
        {
            if (string.IsNullOrEmpty(_idInputField.text) || string.IsNullOrEmpty(_pwInputField.text))
                return;


            (bool isCheckResult, string message) =  await _writeGoogleSheet.WriteToGoogleSheet(_idInputField.text,_pwInputField.text);

            if(isCheckResult == false)            {
                _alertPopup = ShowAlertDialogUI<UIAlertDialog>(_alertPopup, "오류", message);
            }
            else
            {
                _confirmPopup = ShowAlertDialogUI<UIConfirmDialog>(_confirmPopup, "성공", message, ShowLoginAfterSignUp);
                ClearIDAndPw();
            }
        }

        public void ClearIDAndPw()
        {
            _idInputField.text = "";
            _pwInputField.text = "";
        }

        public void ShowLoginAfterSignUp()
        {
            _uiManagerServices.CloseAllPopupUI();
            UILoginPopup uiLoginPopup = _uiManagerServices.GetImportant_Popup_UI<UILoginPopup>();
            _uiManagerServices.ShowPopupUI(uiLoginPopup);
        }

        private UIAlertPopupBase ShowAlertDialogUI<T>(UIAlertPopupBase alertBasePopup,string titleMessage,string bodyText,UnityAction closeButtonAction = null) where T: UIAlertPopupBase
        {
            if(alertBasePopup == null)
            {
                alertBasePopup = _uiManagerServices.GetPopupInDict<T>();
            }
            alertBasePopup.SetText(titleMessage, bodyText);
            if (closeButtonAction != null)
            {
                alertBasePopup.SetCloseButtonOverride(closeButtonAction);
            }
            _uiManagerServices.ShowPopupUI(alertBasePopup);

            return alertBasePopup;
        }

        protected override void StartInit()
        {
        }

        public void OnClickCloseButton()
        {
            _uiManagerServices.ClosePopupUI(this);
        }
    }
}
