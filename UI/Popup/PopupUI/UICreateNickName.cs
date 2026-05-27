using System;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.LobbyManagement;
using GameManagers.LoginManagement;
using GameManagers.SceneManagement;
using GameManagers.UIManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UI.Scene.SceneUI;
using Util;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UICreateNickName : UIPopup
    {
        [Inject] private IUIManagerServices _uiManager;
        [Inject] private ILoginService _loginService;
        [Inject] private LobbyManager _lobbyManager;
        [Inject] private SceneManagerEx _sceneManagerEx;

        enum InputFields
        {
            NickNameInputField
        }

        enum Buttons
        {
            ConfirmButton
        }

        private TMP_InputField _nickNameInputField;
        private Button _confirmButton;
        private UILoadingProgress _uiLoadingProgress;

        private UILoadingProgress UILoadingProgress
        {
            get
            {
                if (_uiLoadingProgress == null)
                {
                    _uiLoadingProgress = _uiManager.GetOrCreateSceneUI<UILoadingProgress>();
                }

                return _uiLoadingProgress;
            }
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _nickNameInputField.text = "";
            _confirmButton.interactable = true;
        }

        protected override void StartInit()
        {
        }

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<TMP_InputField>(typeof(InputFields));
            Bind<Button>(typeof(Buttons));
            _nickNameInputField = Get<TMP_InputField>((int)InputFields.NickNameInputField);
            _confirmButton = Get<Button>((int)Buttons.ConfirmButton);
            _confirmButton.onClick.AddListener(CreateNickname);
        }

        public void CreateNickname()
        {
            _confirmButton.interactable = false;
            CreateUserNickName(_nickNameInputField.text).Forget();
        }

        public async UniTaskVoid CreateUserNickName(string nickname)
        {
            UILoadingProgress.SetText("닉네임 생성 중입니다", "닉네임 중복 여부와 프로필 정보를 저장하고 있습니다");
            UILoadingProgress.ShowLoading();

            LoginResult loginResult;

            try
            {
                loginResult = await _loginService.SaveNickNameAsync(nickname);
            }
            catch (Exception e)
            {
                UtilDebug.LogError($"[CreateNickName] Error: {e}");
                _confirmButton.interactable = true;
                _uiManager.GetMessageErrorToast().Show("닉네임 생성 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요.");
                return;
            }
            finally
            {
                UILoadingProgress.HideLoading();
            }

            if (loginResult.Success == false)
            {
                _uiManager.GetMessageErrorToast().Show(GetCreateNickNameErrorMessage(loginResult.ErrorCode), () => _confirmButton.interactable = true);
                return;
            }

            _uiManager.ClosePopupUI(this);
            await EnterLobbySceneAsync();
        }

        private string GetCreateNickNameErrorMessage(string errorCode)
        {
            switch (errorCode)
            {
                case LoginErrorCode.MissingNickname:
                    return "닉네임을 입력해주세요.";
                case LoginErrorCode.NicknameAlreadyExists:
                    return "이미 해당 닉네임이 존재합니다.";
                case LoginErrorCode.SteamUnavailable:
                    return "Steam을 실행하고 로그인한 뒤 다시 시도해주세요.";
                case LoginErrorCode.NetworkError:
                    return "네트워크 연결을 확인해주세요.";
                case LoginErrorCode.SteamAuthFailed:
                case LoginErrorCode.InvalidSteamTicket:
                case LoginErrorCode.SteamApiError:
                    return "Steam 인증에 실패했습니다. Steam을 재시작한 뒤 다시 시도해주세요.";
                case LoginErrorCode.ServerError:
                    return "서버 오류가 발생했습니다. 잠시 후 다시 시도해주세요.";
                default:
                    return "닉네임 생성 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요.";
            }
        }

        private async UniTask EnterLobbySceneAsync()
        {
            _sceneManagerEx.LoadSceneWithLoadingScreen(Define.SceneName.LobbyScene);

            try
            {
                bool isAlreadyConnected = await _lobbyManager.InitLobbyScene();

                if (isAlreadyConnected == false)
                {
                    return;
                }

                if (_uiManager.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == true)
                {
                    dialog.GetComponent<Canvas>().sortingOrder = 100;
                    dialog.AfterAlertEvent(() => _sceneManagerEx.LoadScene(Define.SceneName.LoginScene))
                        .AlertSetText("접속 오류", "이미 ID가 로비에 접속중입니다. \n 잠시후 다시 접속해주세요.");
                }
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"[CreateNickName] Lobby login failed: {ex}");

                if (_uiManager.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == true)
                {
                    dialog.GetComponent<Canvas>().sortingOrder = 100;
                    dialog.AfterAlertEvent(() => _sceneManagerEx.LoadScene(Define.SceneName.LoginScene))
                        .AlertSetText("접속 오류", "로비에 접근할 수 없습니다. 다시 접속해주세요.");
                }
            }
        }
    }
}
