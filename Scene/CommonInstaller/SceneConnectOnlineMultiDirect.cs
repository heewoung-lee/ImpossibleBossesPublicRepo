using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.Interface.LoginManager;
using GameManagers.RelayManager;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Util;
using Zenject;

namespace Scene.CommonInstaller
{
    public class SceneConnectOnlineMultiDirect : ISceneConnectOnline
    {
        private readonly RelayManager _relayManager;
        private readonly LobbyManager _lobbyManager;
        [Inject]
        public SceneConnectOnlineMultiDirect(RelayManager relayManager, LobbyManager lobbyManager)
        {
            _relayManager = relayManager;
            _lobbyManager = lobbyManager;
        }


        public async UniTask SceneConnectOnlineStart()
        {
            PlayerIngameLoginInfo playerinfo = new PlayerIngameLoginInfo(TestMultiUtil.GetPlayerTag().ToString(),"testMode");
            _lobbyManager.SetPlayerLoginInfo(playerinfo);
            Debug.LogWarning("[SceneConnectLocalDirect] 로컬 리바인딩 적용됨: 로비 과정을 생략합니다.");
            PlayersTag playerType = TestMultiUtil.GetPlayerTag();

            if (playerType == PlayersTag.Player1)
            {
                // 호스트 시작
                // RelayManager가 이미 LocalTestConnection을 쓰고 있으므로, 
                // StartHostAsync(8) 호출 시 내부적으로 Transport 127.0.0.1 설정 후 시작됨
                await _relayManager.StartHostWithRelay(8); 
            }
            else
            {
                // 클라이언트 접속
                await UniTask.Delay(1000); // 호스트 켜질 시간 대기
                _relayManager.JoinLocal(); // 이전에 만든 로컬 접속 함수 (IP: 127.0.0.1)
            }
        }
    }
}
