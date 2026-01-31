using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.Scene;
using TMPro;
using UI.Popup.PopupUI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.SubItem
{
    public class UIRoomInfoPanel : UIBase
    {
        private IUIManagerServices _uIManagerServices;
        private LobbyManager _lobbyManager;
        private SceneManagerEx _sceneManagerEx;
        public IUIManagerServices UIManagerServices => _uIManagerServices;
        
        [Inject]
        public void Construct(LobbyManager lobbyManager, SceneManagerEx sceneManagerEx, IUIManagerServices uIManagerServices)
        {
            _lobbyManager = lobbyManager;
            _sceneManagerEx = sceneManagerEx;
            _uIManagerServices = uIManagerServices;
        }
        
        
        
        enum Texts
        {
            RoomNameText,
            CurrentCount
        }
        enum Buttons { JoinButton }



        private TMP_Text _roomNameText;
        private TMP_Text _currentPlayerCount;
        private Button _joinButton;

        private Lobby _lobbyRegisteredPanel;



        public Lobby LobbyRegisteredPanel => _lobbyRegisteredPanel;
        protected override void AwakeInit()
        {
            Bind<TMP_Text>(typeof(Texts));
            Bind<Button>(typeof(Buttons));
            _roomNameText = Get<TMP_Text>((int)Texts.RoomNameText);
            _currentPlayerCount = Get<TMP_Text>((int)Texts.CurrentCount);
            _joinButton = Get<Button>((int)Buttons.JoinButton);
            _joinButton.onClick.AddListener(() =>
            {
                _joinButton.interactable = false;
                AddJoinEvent().Forget();
            });
        }

        protected override void StartInit()
        {
        }

        public void JoinButtonInteractive(bool isInteractive)
        {
            _joinButton.interactable = isInteractive;
        }
        public void SetRoomInfo(Lobby lobby)
        {
            _lobbyRegisteredPanel = lobby;
            _roomNameText.text = lobby.Name;
            _currentPlayerCount.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        }


        public async UniTaskVoid AddJoinEvent()
        {
            if (_lobbyRegisteredPanel.HasPassword)
            {
                if (UIManagerServices.TryGetPopupDictAndShowPopup(out UIInputRoomPassWord inputroomPassWord) == true)
                {
                    inputroomPassWord.SetRoomInfoPanel(this);
                }
            }
            else
            {
                try
                {
                    await _lobbyManager.LoadingPanel(async () =>
                    {
                        await _lobbyManager.JoinLobbyByID(_lobbyRegisteredPanel.Id);
                        _sceneManagerEx.LoadScene(Define.Scene.RoomScene);
                    });

                }
                catch (LobbyServiceException notFoundLobby) when(notFoundLobby.Message.Contains("lobby not found")) 
                {
                    string errorMsg = "방이 없습니다.";
                    Debug.Log($"{errorMsg}");

                    if (UIManagerServices.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == true)
                    {
                        dialog.AlertSetText("오류",$"{errorMsg}")
                            .AfterAlertEvent(async() =>
                            {
                                await _lobbyManager.ReFreshRoomList();
                            });
                    }
                    _joinButton.interactable = true;
                    _lobbyManager.TriggerLobbyLoadingEvent(false);
                }
            }
        }

    }
}
