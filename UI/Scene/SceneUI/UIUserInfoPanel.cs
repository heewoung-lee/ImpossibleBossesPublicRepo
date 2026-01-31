using System;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.Interface.LoginManager;
using GameManagers.Scene;
using TMPro;
using UI.Popup.PopupUI;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UIUserInfoPanel : UIScene
    {
        private IUIManagerServices _uiManagerServices;
        private IPlayerIngameLogininfo _playerIngameLogininfo;
        private LobbyManager _lobbyManager;
        private SceneManagerEx _sceneManagerEx;
        private SocketEventManager _socketEventManager;

        [Inject]
        public void Construct(IUIManagerServices uiManagerServices, IPlayerIngameLogininfo playerIngameLogininfo, LobbyManager lobbyManager, SceneManagerEx sceneManagerEx, SocketEventManager socketEventManager)
        {
            _uiManagerServices = uiManagerServices;
            _playerIngameLogininfo = playerIngameLogininfo;
            _lobbyManager = lobbyManager;
            _sceneManagerEx = sceneManagerEx;
            _socketEventManager = socketEventManager;
        }
        
        enum Buttons
        {
            CreateRoomButton,
            RefreshLobbyButton,
            LoginSceneBackButton
        }

        enum Texts
        {
            PlayerNickNameText
        }


        Button _createRoomButton;
        Button _refreshLobbyButton;
        Button _loginSceneBackButton;
        UICreateRoom _createRoomUI;

        TMP_Text _userNickNamaText;

        

        private PlayerIngameLoginInfo PlayerIngameLoginInfo => _playerIngameLogininfo.GetPlayerIngameLoginInfo();
        
        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Button>(typeof(Buttons));
            Bind<TMP_Text>(typeof(Texts));
            _createRoomButton = Get<Button>((int)Buttons.CreateRoomButton);
            _createRoomButton.onClick.AddListener(ShowCreateRoomUI);
            _refreshLobbyButton = Get<Button>((int)Buttons.RefreshLobbyButton);
            _refreshLobbyButton.onClick.AddListener(()=>RefreshButton().Forget());
            _loginSceneBackButton = Get<Button>((int)Buttons.LoginSceneBackButton);
            _loginSceneBackButton.onClick.AddListener(() => MoveLoginScene().Forget());
            _userNickNamaText = Get<TMP_Text>((int)Texts.PlayerNickNameText);

        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            ButtonDisInteractable();
            ShowUserNickName();
        }

        protected override void StartInit()
        {
            base.StartInit();
            InitButtonInteractable();
        }

        public async UniTaskVoid RefreshButton()
        {
            _refreshLobbyButton.interactable = false;
            UIRoomInventory inventory = _uiManagerServices.Get_Scene_UI<UIRoomInventory>();
            try
            {
                await _lobbyManager.ReFreshRoomList();
                await _lobbyManager.ShowUpdatedLobbyPlayers();
                _lobbyManager.ShowLobbyData();
                //_relayManager.ShowRelayPlayer();
            }
            catch (Exception ex)
            {
                if (_uiManagerServices.TryGetPopupDictAndShowPopup(out UIAlertDialog alertPopup) == true)
                {
                    alertPopup.SetText("오류", $"{ex}");
                }

            }
            finally
            {
            _refreshLobbyButton.interactable = true;
                
            }
            GetActiveVivoxChannels();
        }


        public void GetActiveVivoxChannels()
        {
            var activeChannels = VivoxService.Instance.ActiveChannels;

            if (activeChannels.Count == 0)
            {
                Debug.Log("현재 접속 중인 채널이 없습니다.");
                return;
            }

            Debug.Log($"현재 접속 중인 VIVOX 채널 수: {activeChannels.Count}");

            foreach (var channel in activeChannels)
            {
                string channelName = channel.Key; // 채널 ID 또는 이름
                var channelSession = channel.Value; // 채널 세션 정보

                Debug.Log($"채널 이름: {channelName}");
            }
        }
        private void InitButtonInteractable()
        {
            if (_lobbyManager.IsDoneLobbyInitEvent == false)
            {
                _lobbyManager.InitDoneEvent += ButtonInteractable;
            }
            else
            {
                ButtonInteractable();
            }
        }
        public void ShowCreateRoomUI()
        {
            if (_createRoomUI == null)
            {
                _createRoomUI = _uiManagerServices.GetPopupUIFromResource<UICreateRoom>();
            }
            _uiManagerServices.ShowPopupUI(_createRoomUI);
        }

        private void ShowUserNickName()
        {
            if (PlayerIngameLoginInfo.Equals(default(PlayerIngameLoginInfo)))
            {
                _lobbyManager.InitDoneEvent += ShowNickname;
            }
            else
            {
                ShowNickname();
            }
        }

        private void ShowNickname() 
        {
            _userNickNamaText.text += PlayerIngameLoginInfo.PlayerNickName;
        }

        public async UniTaskVoid MoveLoginScene()
        {
            try
            {
                await _socketEventManager.InvokeLogoutAllLeaveLobbyEvent();
                await _socketEventManager.InvokeDisconnectRelayEvent();
                await _socketEventManager.InvokeLogoutVivoxEvent();
            }
            catch (Exception e)
            {
                Debug.Log($"에러가 발생했습니다.{e}");
                return;
            }
            _sceneManagerEx.LoadScene(Define.Scene.LoginScene);
        }

        private void ButtonInteractable()
        {
            _createRoomButton.interactable = true;
            _refreshLobbyButton.interactable = true;
            _loginSceneBackButton.interactable = true;
        }
        private void ButtonDisInteractable()
        {
            _createRoomButton.interactable = false;
            _refreshLobbyButton.interactable = false;
            _loginSceneBackButton.interactable = false;
        }

    }
}
