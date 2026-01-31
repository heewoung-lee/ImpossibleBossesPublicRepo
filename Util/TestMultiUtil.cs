using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers.Interface.LoginManager;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Scene.RoomScene;
using Unity.Multiplayer.Playmode;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace Util
{
    public static class TestMultiUtil
    {
        public const string LobbyName = "TestLobby";
        
        
        public static async UniTask<PlayerIngameLoginInfo> SetAuthenticationService(PlayersTag playerTag)
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            string playerID = AuthenticationService.Instance.PlayerId;
            
            return new PlayerIngameLoginInfo(playerTag.ToString(), playerID);
        }


        public static PlayersTag GetPlayerTag()
        {
            string[] tagValue = CurrentPlayer.ReadOnlyTags();

            PlayersTag currentPlayer = PlayersTag.Player1;
            if (tagValue.Length > 0 && Enum.TryParse(typeof(PlayersTag), tagValue[0], out var parsedEnum))
            {
                currentPlayer = (PlayersTag)parsedEnum;
            }

            return currentPlayer;
        }
    }
}
