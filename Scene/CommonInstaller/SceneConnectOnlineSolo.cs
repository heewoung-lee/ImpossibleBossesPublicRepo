using System.Threading.Tasks;
using GameManagers;
using GameManagers.Interface.LoginManager;
using Scene.CommonInstaller.Interfaces;
using Util;
using Zenject;

namespace Scene.CommonInstaller
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
        public async Task SceneConnectOnlineStart()
        {
            PlayerIngameLoginInfo playerinfo = await TestMultiUtil.SetAuthenticationService(TestMultiUtil.GetPlayerTag());
            _lobbyManager.SetPlayerLoginInfo(playerinfo);
            await _relayManager.StartHostWithRelay(8);
            _lobbyManager.InitializeLobbyEvent();
            _relayManager.SpawnToRPC_Caller();
        }
    }
}
