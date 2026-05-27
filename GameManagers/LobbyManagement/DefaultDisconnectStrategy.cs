using System;
using Cysharp.Threading.Tasks;
using GameManagers.RelayManagement;
using GameManagers.SceneManagement;
using ScenesScripts.CommonInstaller.Interfaces;
using Util;
using Zenject;

namespace GameManagers.LobbyManagement
{
    public class DefaultDisconnectStrategy : IDisconnectStrategy
    {
        public UniTask HandleDisconnectAsync(ulong disconnectID, RelayManager relayManager, LobbyManager lobbyManager,
            SceneManagerEx sceneManger)
        {
            if (relayManager.NetworkManagerEx.LocalClientId != disconnectID) return UniTask.CompletedTask;
     
            sceneManger.LoadSceneWithLoadingScreen(Define.SceneName.LobbyScene);
            UniTaskExtensions.Forget(lobbyManager.InitLobbyScene());
            
            return UniTask.CompletedTask;
        }
    }
}
