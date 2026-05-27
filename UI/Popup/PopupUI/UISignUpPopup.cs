using GameManagers.UIManagement;
using TMPro;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UISignUpPopup : IDPwPopup, IUIHasCloseButton
    {
        [Inject] private IUIManagerServices _uiManagerServices;

        private Button _buttonClose;
        private Button _buttonSignup;
        private TMP_InputField _idInputField;
        private TMP_InputField _pwInputField;

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
            _buttonSignup.gameObject.SetActive(false);
        }

        public void CreateID()
        {
            ShowSignUpDisabledMessage();
        }

        public void ClearIDAndPw()
        {
            _idInputField.text = "";
            _pwInputField.text = "";
        }

        public void ShowLoginAfterSignUp()
        {
            _uiManagerServices.ClosePopupUI(this);
        }

        protected override void StartInit()
        {
        }

        public void OnClickCloseButton()
        {
            _uiManagerServices.ClosePopupUI(this);
        }

        private void ShowSignUpDisabledMessage()
        {
            _uiManagerServices.GetMessageErrorToast().Show("회원가입은 더 이상 사용하지 않습니다.");
        }
    }
}
