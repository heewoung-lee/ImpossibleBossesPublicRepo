using System;
using Cysharp.Threading.Tasks;
using GameManagers.RelayManagement;
using GameManagers.SceneManagement;
using GameManagers.UIManagement;
using ScenesScripts.CommonInstaller.Interfaces;
using UI.Popup.PopupUI;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers.LobbyManagement
{
    public class RoomMigrationStrategy : IDisconnectStrategy,IInitializable,IDisposable
    {
        private const string HostDisconnectedTitle = "접속 오류";
        private const string HostDisconnectedBody = "파티장이 나가 방 접속이 중단되었습니다.\n로비로 돌아갑니다.";

        private readonly IRegistrar<IDisconnectStrategy> _registrar;
        private readonly IUIManagerServices _uiManagerServices;

        [Inject]
        public RoomMigrationStrategy(
            IRegistrar<IDisconnectStrategy> registrar,
            IUIManagerServices uiManagerServices)
        {
            _registrar = registrar;
            _uiManagerServices = uiManagerServices;
        }

        public void Initialize()
        {
            _registrar.Register(this);
        }
        public void Dispose()
        {
            _registrar.Unregister(this);
        }


        public  UniTask HandleDisconnectAsync(ulong disconnectID, RelayManager relayManager, LobbyManager lobbyManager,
            SceneManagerEx sceneManger)
        {
            if (relayManager.NetworkManagerEx.LocalClientId != disconnectID) return UniTask.CompletedTask;
            
            //내가 의도적으로 나간 경우라면, 바로 로비로 이동
            if (relayManager.DisconnectCause == RelayDisconnectCause.IntentionalLeaveToLobby)
            {
                MoveToLobbyAsync().Forget();
            }
            else
            {
                relayManager.ShutDownRelay(RelayDisconnectCause.MigrationRecovery);

                if (_uiManagerServices.Try_Get_Scene_UI(out UIRoomCharacterSelect roomCharacterSelect) == false)
                {
                    ShowHostDisconnectedDialog();
                    return UniTask.CompletedTask;
                }

                if (roomCharacterSelect.IsReadyForHostMigration == false)
                {
                    roomCharacterSelect.ShowRoomEntryFailedDialog();
                    return UniTask.CompletedTask;
                }

                lobbyManager.TriggerLobbyLoadingEvent(true);
            }
            return UniTask.CompletedTask;

            void ShowHostDisconnectedDialog()
            {
                if (_uiManagerServices.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == false)
                {
                    MoveToLobbyAsync().Forget();
                    return;
                }

                Canvas canvas = dialog.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 100;
                }

                dialog.AfterAlertEvent(() => MoveToLobbyAsync().Forget())
                    .AlertSetText(HostDisconnectedTitle, HostDisconnectedBody);
            }

            async UniTask MoveToLobbyAsync()
            {
                sceneManger.LoadSceneWithLoadingScreen(Define.SceneName.LobbyScene);
                await lobbyManager.TryJoinLobbyByNameOrCreateWaitLobby();
            }
        }
    }
}
