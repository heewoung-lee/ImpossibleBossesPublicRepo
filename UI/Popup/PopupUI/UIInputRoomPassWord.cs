using System;
using Cysharp.Threading.Tasks;
using GameManagers.LobbyManagement;
using GameManagers.SceneManagement;
using GameManagers.UIManagement;
using TMPro;
using UI.SubItem;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UIInputRoomPassWord : UIPopup
    {
        private IUIManagerServices _uiManagerServices;
        private LobbyManager _lobbyManager;
        private SceneManagerEx _sceneManagerEx;

        [Inject]
        public void Construct(
            IUIManagerServices uiManagerServices,
            LobbyManager lobbyManager,
            SceneManagerEx sceneManagerEx)
        {
            _uiManagerServices = uiManagerServices;
            _lobbyManager = lobbyManager;
            _sceneManagerEx = sceneManagerEx;
        }

        enum InputFields
        {
            RoomPassWordInputField
        }

        enum Buttons
        {
            ConfirmButton
        }

        private TMP_InputField _roomPwInputField;
        private UIRoomInfoPanel _roomInfoPanel;
        private Button _confirmButton;

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _roomPwInputField.text = "";
            _confirmButton.interactable = true;

            if (_roomInfoPanel != null)
            {
                _roomInfoPanel.JoinButtonInteractive(true);
            }

            _roomInfoPanel = null;
        }

        protected override void StartInit()
        {
        }

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<TMP_InputField>(typeof(InputFields));
            Bind<Button>(typeof(Buttons));
            _roomPwInputField = Get<TMP_InputField>((int)InputFields.RoomPassWordInputField);
            _confirmButton = Get<Button>((int)Buttons.ConfirmButton);
            _confirmButton.onClick.AddListener(() => CheckJoinRoom().Forget());
        }

        public void SetRoomInfoPanel(UIRoomInfoPanel infoPanel)
        {
            _roomInfoPanel = infoPanel;
        }

        private async UniTaskVoid CheckJoinRoom()
        {
            _confirmButton.interactable = false;
            Lobby lobby = _roomInfoPanel.LobbyRegisteredPanel;
            Lobby joinedLobby = null;

            try
            {
                await _lobbyManager.LoadingPanel(async () =>
                {
                    joinedLobby = await _lobbyManager.JoinLobbyByID(lobby.Id, _roomPwInputField.text);
                });
            }
            catch (Unity.Services.Lobbies.LobbyServiceException wrongPw) when
                (wrongPw.Reason == Unity.Services.Lobbies.LobbyExceptionReason.IncorrectPassword ||
                 wrongPw.Reason == LobbyExceptionReason.ValidationError)
            {
                _uiManagerServices.GetMessageErrorToast().Show("비밀번호가 틀렸습니다", () => _confirmButton.interactable = true);
                return;
            }
            catch (LobbyServiceException notfound) when (notfound.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                UtilDebug.Log("로비를 찾을 수 없습니다");

                if (_uiManagerServices.TryGetPopupDictAndShowPopup(out UIAlertDialog loginPopup) == true)
                {
                    loginPopup.AlertSetText("오류", "로비를 찾을 수 없습니다")
                        .AfterAlertEvent(async () =>
                        {
                            _uiManagerServices.ClosePopupUI(this);
                            _confirmButton.interactable = true;
                            await _lobbyManager.ReFreshRoomList();
                        });
                }

                return;
            }
            catch (Exception error)
            {
                UtilDebug.Log($"에러가 발생했습니다{error}");
                _confirmButton.interactable = true;
                return;
            }
            finally
            {
                _lobbyManager.TriggerLobbyLoadingEvent(false);
            }

            if (joinedLobby == null)
            {
                _confirmButton.interactable = true;
                return;
            }

            _sceneManagerEx.LoadScene(Define.SceneName.RoomScene);
        }
    }
}
