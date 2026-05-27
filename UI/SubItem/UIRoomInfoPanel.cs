using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.LobbyManagement;
using GameManagers.SceneManagement;
using GameManagers.UIManagement;
using TMPro;
using UI.Popup.PopupUI;
using UI.Scene.SceneUI;
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
        private const string JoinLoadingTitle = "방에 접속중";
        private const string JoinLoadingBody = "방 정보를 확인하고 캐릭터 선택창으로 이동하고 있습니다.";

        private IUIManagerServices _uIManagerServices;
        private LobbyManager _lobbyManager;
        private SceneManagerEx _sceneManagerEx;
        private UILoadingProgress _uiLoadingProgress;
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
        private UILoadingProgress UILoadingProgress
        {
            get
            {
                if (_uiLoadingProgress == null)
                {
                    _uiLoadingProgress = UIManagerServices.GetOrCreateSceneUI<UILoadingProgress>();
                }

                return _uiLoadingProgress;
            }
        }

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
                UILoadingProgress.ShowLoading(JoinLoadingTitle, JoinLoadingBody);
                try
                {
                    await _lobbyManager.LoadingPanel(async () =>
                    {
                        UtilDebug.Log("버튼을 눌렀을때 넘어가는 메서드");
                        Lobby joinedLobby = await _lobbyManager.JoinLobbyByID(_lobbyRegisteredPanel.Id);
                        if (joinedLobby == null)
                        {
                            _joinButton.interactable = true;
                            return;
                        }

                        _sceneManagerEx.LoadScene(Define.SceneName.RoomScene);
                    });

                }
                catch (LobbyServiceException notFoundLobby) when(notFoundLobby.Message.Contains("lobby not found")) 
                {
                    string errorMsg = "방이 없습니다.";
                    UtilDebug.Log($"{errorMsg}");

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
                finally
                {
                    UILoadingProgress.HideLoading();
                }
            }
        }

    }
}
