#if UNITY_EDITOR
using System;
using System.Text;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.LobbyManagement;
using GameManagers.LoginManagement;
using GameManagers.SceneManagement;
using GameManagers.UIManagement;
using UI.Popup.PopupUI;
using UI.Scene;
using Unity.Multiplayer.Playmode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Util;
using Zenject;

namespace Test.TestUI
{
    public class LogInTestToggle : UIScene
    {
        private const string GetTestProfileUrl = "https://us-central1-impossiblebosses-43b63.cloudfunctions.net/getTestProfile";

        [Inject] private IUIManagerServices _uiManager;
        [Inject] private LobbyManager _lobbyManager;
        [Inject] private SceneManagerEx _sceneManagerEx;
        [Inject] private SteamFirebaseLoginService _steamFirebaseLoginService;

        enum Buttons
        {
            TestButton
        }
        enum Toggles 
        {
            LogInTestToggle
        }

        enum Players
        {
            Player1,
            Player2,
            Player3,
            Player4,
            None
        }


        Button _testButton;
        Toggle _testToggle;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Button>(typeof(Buttons));
            Bind<Toggle>(typeof(Toggles));
            _testButton = Get<Button>((int)Buttons.TestButton);
            _testToggle = Get<Toggle>((int)Toggles.LogInTestToggle);
            _testButton.interactable = _testToggle.interactable;
            _testToggle.onValueChanged.AddListener((ison) =>
            {
                _testButton.gameObject.SetActive(ison);
            });
            _testButton.onClick.AddListener(() => ClickLogin().Forget());
        }

        private async UniTaskVoid ClickLogin()
        {
            _testButton.interactable = false;
            Players currentPlayer = GetCurrentPlayer();
            string testPlayerId = GetTestPlayerId(currentPlayer);

            if (string.IsNullOrEmpty(testPlayerId))
            {
                _testButton.interactable = true;
                return;
            }

            TestProfileResponse profileResponse = await GetTestProfileAsync(testPlayerId);

            if (profileResponse == null || profileResponse.success == false)
            {
                ShowTestLoginError(profileResponse?.code ?? "TEST_LOGIN_FAILED");
                _testButton.interactable = true;
                return;
            }

            if (string.IsNullOrEmpty(profileResponse.NickName))
            {
                ShowTestLoginError("TEST_PROFILE_NICKNAME_MISSING");
                _testButton.interactable = true;
                return;
            }

            _steamFirebaseLoginService.SetEditorTestProfile(testPlayerId, profileResponse.NickName);
            await EnterLobbySceneAsync();
        }

        private Players GetCurrentPlayer()
        {
            Players currentPlayer = Players.Player1;

            string[] tagValue = CurrentPlayer.ReadOnlyTags();

            if (tagValue.Length > 0 && Enum.TryParse(tagValue[0], out Players parsedEnum))
            {
                currentPlayer = parsedEnum;
                UtilDebug.Log($"Current player: {currentPlayer}");
            }

            return currentPlayer;
        }

        private string GetTestPlayerId(Players currentPlayer)
        {
            switch (currentPlayer)
            {
                case Players.Player1:
                    return "Player1";
                case Players.Player2:
                    return "Player2";
                case Players.Player3:
                    return "Player3";
                case Players.Player4:
                    return "Player4";
                default:
                    return string.Empty;
            }
        }

        private async UniTask<TestProfileResponse> GetTestProfileAsync(string testPlayerId)
        {
            string url = $"{GetTestProfileUrl}?SteamID64={UnityWebRequest.EscapeURL(testPlayerId, Encoding.UTF8)}";
            UnityWebRequest request = UnityWebRequest.Get(url);

            await request.SendWebRequest().ToUniTask();

            string responseText = request.downloadHandler.text;

            if (request.result != UnityWebRequest.Result.Success)
            {
                UtilDebug.LogError($"[LogInTestToggle] Firebase test login failed. Result: {request.result}, Error: {request.error}, Body: {responseText}");
                request.Dispose();
                return null;
            }

            TestProfileResponse profileResponse = JsonUtility.FromJson<TestProfileResponse>(responseText);
            request.Dispose();
            return profileResponse;
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
                        .AlertSetText("접속 오류", "이미 접속 중입니다.");
                }
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"[LogInTestToggle] Lobby login failed: {ex}");

                if (_uiManager.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == true)
                {
                    dialog.AfterAlertEvent(() => _sceneManagerEx.LoadScene(Define.SceneName.LoginScene))
                        .AlertSetText("Error", "Lobby login failed.");
                }
            }
        }

        private void ShowTestLoginError(string message)
        {
            _uiManager.GetMessageErrorToast().Show(message);
        }

        [Serializable]
        private class TestProfileResponse
        {
            public bool success;
            public string code;
            public string SteamID64;
            public string NickName;
        }
    }
}
#endif
