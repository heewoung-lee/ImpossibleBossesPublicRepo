using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.LobbyManagement;
using GameManagers.LoginManagement;
using GameManagers.SceneManagement;
using GameManagers.SoundManagement;
using GameManagers.UIManagement;
using UI.Popup.PopupUI;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UILoginTitle : UIScene
    {
        [Inject] private IUIManagerServices _uiManager;
        [Inject] private ILoginService _loginService;
        [Inject] private LobbyManager _lobbyManager;
        [Inject] private SceneManagerEx _sceneManagerEx;

        enum ButtonEvent
        {
            ButtonStart,
            ButtonSetting,
            ButtonQuit,
        }

        private Button _openLoginButton;
        private Button _settingButton;
        private Button _quitButton;
        private UICreateNickName _uiCreateNickName;
        private UILoadingProgress _uiLoadingProgress;
        private bool _isLoginProcessing;

        private UICreateNickName UICreateNickName
        {
            get
            {
                if (_uiCreateNickName == null)
                {
                    _uiCreateNickName = _uiManager.GetPopupInDict<UICreateNickName>();
                }

                return _uiCreateNickName;
            }
        }

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

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Button>(typeof(ButtonEvent));
            _openLoginButton = Get<Button>((int)ButtonEvent.ButtonStart);
            _settingButton = Get<Button>((int)ButtonEvent.ButtonSetting);
            _quitButton = Get<Button>((int)ButtonEvent.ButtonQuit);
        }

        protected override void StartInit()
        {
            base.StartInit();
            _openLoginButton.onClick.AddListener(() => ClickLoginButton().Forget());
            BindButtonSound(_openLoginButton);
            BindButtonSound(_settingButton);
            BindButtonSound(_quitButton);
            UILoadingProgress.HideLoading();
        }
    

        public async UniTask ClickLoginButton()
        {
            if (_isLoginProcessing)
            {
                return;
            }

            _isLoginProcessing = true;
            _openLoginButton.interactable = false;
            UILoadingProgress.SetText("로그인 중입니다", "Steam 인증과 프로필 정보를 확인하고 있습니다");
            UILoadingProgress.ShowLoading();

            LoginResult loginResult;

            try
            {
                loginResult = await _loginService.LoginAsync();
            }
            catch (System.Exception ex)
            {
                UtilDebug.LogError($"[UILoginTitle] Login failed: {ex}");
                loginResult = LoginResult.Fail(LoginErrorCode.ServerError);
            }
            finally
            {
                UILoadingProgress.HideLoading();
            }

            if (loginResult.Success == false)
            {
                UtilDebug.LogWarning($"[UILoginTitle] Login button failed. ErrorCode: {loginResult.ErrorCode}");
                ShowLoginError(loginResult.ErrorCode);
                ResetLoginButton();
                return;
            }

            if (loginResult.NeedsNickName)
            {
                _uiManager.ShowPopupUI(UICreateNickName);
                ResetLoginButton();
                return;
            }

            await EnterLobbySceneAsync();
        }

        private void BindButtonSound(Button button)
        {
            if (button == null)
            {
                return;
            }

            BindEvent(button.gameObject, _ =>
            {
                if (button.interactable)
                {
                    _soundManagerServices.PlayUiSfx(button.gameObject, UICommonSoundCueId.Hover);
                }
            }, Define.UIEvent.PointerEnter);
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
            catch (System.Exception ex)
            {
                UtilDebug.LogError($"[UILoginTitle] Lobby login failed: {ex}");

                if (_uiManager.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == true)
                {
                    dialog.GetComponent<Canvas>().sortingOrder = 100;
                    dialog.AfterAlertEvent(() => _sceneManagerEx.LoadScene(Define.SceneName.LoginScene))
                        .AlertSetText("접속 오류", "로비에 접근할 수 없습니다. 다시 접속해주세요.");
                }
            }
        }

        private void ShowLoginError(string errorCode)
        {
            if (_uiManager.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == false)
            {
                return;
            }

            dialog.AlertSetText("로그인 실패", GetLoginErrorMessage(errorCode));
        }

        private string GetLoginErrorMessage(string errorCode)
        {
            switch (errorCode)
            {
                case LoginErrorCode.SteamUnavailable:
                    return "Steam을 실행하고 로그인한 뒤 다시 시도해주세요";
                case LoginErrorCode.NetworkError:
                    return "네트워크 연결을 확인해주세요";
                case LoginErrorCode.SteamAuthFailed:
                case LoginErrorCode.InvalidSteamTicket:
                case LoginErrorCode.SteamApiError:
                    return "Steam 인증에 실패했습니다. Steam을 재시작한 뒤 다시 시도해주세요";
                case LoginErrorCode.ServerError:
                    return "서버 오류가 발생했습니다. 잠시 후 다시 시도해주세요";
                default:
                    return "로그인 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요";
            }
        }

        private void ResetLoginButton()
        {
            _isLoginProcessing = false;
            _openLoginButton.interactable = true;
        }


    }
}
