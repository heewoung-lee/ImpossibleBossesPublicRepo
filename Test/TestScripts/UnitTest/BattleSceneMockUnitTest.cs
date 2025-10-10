using System;
using System.Threading.Tasks;
using GameManagers;
using GameManagers.Interface.LoginManager;
using GameManagers.Interface.UIManager;
using NetWork.NGO.UI;
using Scene;
using Scene.BattleScene.Spawner;
using Scene.GamePlayScene;
using UI.Scene.SceneUI;
using Unity.Multiplayer.Playmode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Util;
using Zenject;

namespace Test.TestScripts.UnitTest
{
    public class BattleSceneMockUnitTest : BaseScene
    {
        private IUIManagerServices _uiManagerServices;
        private LobbyManager _lobbyManager;
        private RelayManager _relayManager;
        private NgoPoolManager _poolManager;
        
        [Inject]
        public void Construct(
            IUIManagerServices uiManagerServices,
            LobbyManager lobbyManager,
            RelayManager relayManager,
            NgoPoolManager poolManager)
        {
            _uiManagerServices = uiManagerServices;
            _lobbyManager = lobbyManager;
            _relayManager = relayManager;
            _poolManager = poolManager;
        }
        public enum PlayersTag
        {
            Player1,
            Player2,
            Player3,
            Player4,
            None
        }
        string _playerType = null;
        GameObject _ngoRoot;
        private const string LobbyName = "TestLobby";
        private UILoading _uiLoadingScene;
        public Define.PlayerClass PlayerClass;
        public bool isSoloTest;

        public override Define.Scene CurrentScene => Define.Scene.BattleScene;
        protected override async void StartInit()
        {
            base.StartInit();
            _uiLoadingScene = _uiManagerServices.GetOrCreateSceneUI<UILoading>();
            await JoinChannel();
            //_gameManagerEx.GetPlayer().GetComponent<Module_Player_Class>().InitializeSkillsFromManager();
        }
        private async Task JoinChannel()
        {
            _relayManager.NetworkManagerEx.OnClientConnectedCallback -= ConnectClicent;
            _relayManager.NetworkManagerEx.OnClientConnectedCallback += ConnectClicent;
            if (_relayManager.NetworkManagerEx.IsListening == false)
            {
                await SetAuthenticationService();
                if (_playerType == "Player1")
                {
                    if (isSoloTest == true)//나혼자 테스트 할때
                    {
                        await _relayManager.StartHostWithRelay(8);
                    }
                    else
                    {
                        await _lobbyManager.CreateLobby("TestLobby", 8, null);
                    }
                    if (_relayManager.NetworkManagerEx == true)
                    {
                        Init_NGO_PlayScene_OnHost();
                    }
                }
                else
                {

                    await Task.Delay(1000);
                    Lobby lobby = await _lobbyManager.AvailableLobby(LobbyName);
                    if (lobby.Data == null)
                    {
                        await Utill.RateLimited(async () => await JoinChannel(), 1000);
                        return;
                    }
                    string joinCode = lobby.Data["RelayCode"].Value;
                    await _relayManager.JoinGuestRelay(joinCode);
                }
            }
        }

        private void ConnectClicent(ulong clientID)
        {
            if (_relayManager.NgoRPCCaller == null)
            {
                _relayManager.SpawnRpcCallerEvent += SpawnPlayer;
            }
            else
            {
                SpawnPlayer();
            }
            void SpawnPlayer()
            {
                if (_relayManager.NetworkManagerEx.LocalClientId != clientID)
                    return;
                Define.PlayerClass playerClass =
                    (int)PlayerClass + (int)clientID < Enum.GetValues(typeof(Define.PlayerClass)).Length 
                        ? (Define.PlayerClass)((int)PlayerClass + (int)clientID): Define.PlayerClass.Archer;
                _relayManager.RegisterSelectedCharacter(clientID, playerClass);
                _relayManager.NgoRPCCaller.GetPlayerChoiceCharacterRpc(clientID);
                LoadBattleScene();
            }
        }

        private void LoadBattleScene()
        {
            _uiLoadingScene.gameObject.SetActive(false);
        }
        private async Task SetAuthenticationService()
        {

            _playerType = GetPlayerTag();
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            string playerID = AuthenticationService.Instance.PlayerId;
            _lobbyManager.SetPlayerLoginInfo(new PlayerIngameLoginInfo(_playerType, playerID));
        }

        private void Init_NGO_PlayScene_OnHost()
        {
            if (_relayManager.NetworkManagerEx.IsHost)
            {
                _relayManager.SpawnNetworkObj("Prefabs/NGO/NgoBattleSceneSpawn",_relayManager.NgoRoot.transform);
                _poolManager.Create_NGO_Pooling_Object();
            }
        }
        public string GetPlayerTag()
        {
            string[] tagValue = CurrentPlayer.ReadOnlyTags();

            PlayersTag currentPlayer = PlayersTag.Player1;
            if (tagValue.Length > 0 && Enum.TryParse(typeof(PlayersTag), tagValue[0], out var parsedEnum))
            {
                currentPlayer = (PlayersTag)parsedEnum;
            }
            return Enum.GetName(typeof(PlayersTag), currentPlayer);
        }

        protected override void AwakeInit()
        {
        }

        public override void Clear()
        {
        }
    }
}
