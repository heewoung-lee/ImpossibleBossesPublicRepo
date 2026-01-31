using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.Scene;
using Module.UI_Module;
using TMPro;
using UI.SubItem;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UIInputRoomPassWord : UIPopup
    {
        private IUIManagerServices _uiManagerServices;
        private LobbyManager _lobbyManager;
        SceneManagerEx _sceneManagerEx;
        
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

        enum GameObjects
        {
            MessageError
        }

        private TMP_InputField _roomPwInputField;
        private UIRoomInfoPanel _roomInfoPanel;
        private Button _confirmButton;
        private GameObject _messageError;
        private TMP_Text _errorMessageText;
        private ModuleUIFadeOut _errorMessageTextFadeOutMoudule;

        public PlayerLoginInfo PlayerLoginInfo { get; set; }


        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _roomPwInputField.text = "";
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
            Bind<GameObject>(typeof(GameObjects));
            _roomPwInputField = Get<TMP_InputField>((int)InputFields.RoomPassWordInputField);
            _confirmButton = Get<Button>((int)Buttons.ConfirmButton);
            _messageError = Get<GameObject>((int)GameObjects.MessageError);
            _errorMessageText = _messageError.GetComponentInChildren<TMP_Text>();
            _errorMessageTextFadeOutMoudule = _messageError.GetComponent<ModuleUIFadeOut>();
            _errorMessageTextFadeOutMoudule.DoneFadeoutEvent += () => _confirmButton.interactable = true;
            _confirmButton.onClick.AddListener(()=>CheckJoinRoom().Forget());
            _messageError.SetActive(false);
        }

        public void SetRoomInfoPanel(UIRoomInfoPanel infoPanel)
        {
            _roomInfoPanel = infoPanel;
        }

        private async UniTaskVoid CheckJoinRoom()
        {
            _confirmButton.interactable = false;
            Lobby lobby = _roomInfoPanel.LobbyRegisteredPanel;
            try
            {
                await _lobbyManager.LoadingPanel(async () =>
                {
                    await _lobbyManager.JoinLobbyByID(lobby.Id, _roomPwInputField.text);
                });
            }
            catch (Unity.Services.Lobbies.LobbyServiceException wrongPw) when
                (wrongPw.Reason == Unity.Services.Lobbies.LobbyExceptionReason.IncorrectPassword ||
                 wrongPw.Reason == LobbyExceptionReason.ValidationError)
            {
                _errorMessageText.text = "비밀번호가 틀렸습니다";
                _messageError.SetActive(true);
                return;
            }
            catch (LobbyServiceException notfound) when (notfound.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                Debug.Log("로비를 찾을 수 없습니다");

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
                Debug.Log($"에러가 발생했습니다{error}");
                _confirmButton.interactable = true;
                return;
            }
            finally
            {
                _lobbyManager.TriggerLobbyLoadingEvent(false);
            }


            _sceneManagerEx.LoadScene(Define.Scene.RoomScene);
        }
    }
}