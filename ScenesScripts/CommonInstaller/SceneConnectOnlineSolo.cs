using Cysharp.Threading.Tasks;
using GameManagers.LobbyManagement;
using GameManagers.LoginManagement;
using GameManagers.RelayManagement;
using ScenesScripts.CommonInstaller.Interfaces;
using Util;
using Zenject;

namespace ScenesScripts.CommonInstaller
{
    internal class SceneConnectOnlineSolo: ISceneConnectOnline
    {
        private readonly LobbyManager _lobbyManager;
        private readonly RelayManager _relayManager;

        [Inject]
        public SceneConnectOnlineSolo(LobbyManager lobbyManager, RelayManager relayManager)
        {
            _lobbyManager = lobbyManager;
            _relayManager = relayManager;
        }

        private string _playerType;
        public async UniTask SceneConnectOnlineStart()
        {
            PlayerIngameLoginInfo playerinfo = await TestMultiUtil.SetAuthenticationService(TestMultiUtil.GetPlayerTag());
            _lobbyManager.SetPlayerLoginInfo(playerinfo);
            await _relayManager.StartHostWithRelay(8);
            _lobbyManager.InitializeLobbyEvent();
        }
    }
}
