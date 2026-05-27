using Cysharp.Threading.Tasks;
using GameManagers.UIManagement;
using TMPro;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UILoginPopup : IDPwPopup, IUIHasCloseButton
    {
        [Inject] private IUIManagerServices _uiManagerServices;

        enum Buttons
        {
            CloseButton,
            SignupButton,
            ConfirmButton
        }

        enum InputFields
        {
            IDInputField,
            PwInputField
        }

        private Button _closeButton;
        private Button _signupButton;
        private Button _confirmButton;
        private TMP_InputField _idInputField;
        private TMP_InputField _pwInputField;

        public override TMP_InputField IdInputField => _idInputField;

        public override TMP_InputField PwInputField => _pwInputField;

        public Button CloseButton => _closeButton;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Button>(typeof(Buttons));
            Bind<TMP_InputField>(typeof(InputFields));
            _closeButton = Get<Button>((int)Buttons.CloseButton);
            _signupButton = Get<Button>((int)Buttons.SignupButton);
            _confirmButton = Get<Button>((int)Buttons.ConfirmButton);
            _idInputField = Get<TMP_InputField>((int)InputFields.IDInputField);
            _pwInputField = Get<TMP_InputField>((int)InputFields.PwInputField);
            _closeButton.onClick.AddListener(OnClickCloseButton);
            _signupButton.gameObject.SetActive(false);
            _confirmButton.onClick.AddListener(() => AuthenticateUser(_idInputField.text, _pwInputField.text).Forget());
        }

        protected override void StartInit()
        {
        }

        public void ShowSignUpUI()
        {
            ShowLegacyLoginDisabledMessage();
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            _uiManagerServices.AddImportant_Popup_UI(this);
        }

        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            _idInputField.text = "";
            _pwInputField.text = "";
        }

        public async UniTask AuthenticateUser(string userID, string userPw)
        {
            ShowLegacyLoginDisabledMessage();
            await UniTask.CompletedTask;
        }

        public void OnClickCloseButton()
        {
            _uiManagerServices.ClosePopupUI(this);
        }

        private void ShowLegacyLoginDisabledMessage()
        {
            _uiManagerServices.GetMessageErrorToast().Show("기존 ID/PW 로그인은 더 이상 사용하지 않습니다.");
        }
    }
}
