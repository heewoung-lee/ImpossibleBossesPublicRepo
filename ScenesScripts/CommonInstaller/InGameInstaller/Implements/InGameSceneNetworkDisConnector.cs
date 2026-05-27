using System;
using Cysharp.Threading.Tasks;
using GameManagers.LobbyManagement;
using GameManagers.RelayManagement;
using GameManagers.SceneManagement;
using GameManagers.UIManagement;
using ScenesScripts.CommonInstaller.Interfaces;
using UI.Popup.PopupUI;
using UnityEngine;
using Util;
using Zenject;

namespace ScenesScripts.CommonInstaller.InGameInstaller.Implements
{
    public class InGameSceneNetworkDisConnector : IDisconnectStrategy, IInitializable, IDisposable
    {
        private const string DISCONNECTMENTTITLE = "오류";
        private const string DISCONNECTMENTBOBY = "서버와의 연결이 끊어졌습니다.";
        private readonly IRegistrar<IDisconnectStrategy> _registrar;
        private readonly IUIManagerServices _uiManagerServices;

        private UIAlertDialog _uiAlertDialog;

        [Inject]
        public InGameSceneNetworkDisConnector(
            IRegistrar<IDisconnectStrategy> registrar,
            IUIManagerServices uiManagerServices)
        {
            _registrar = registrar;
            _uiManagerServices = uiManagerServices;
        }

        public void Initialize()
        {
            _registrar.Register(this);

            _uiAlertDialog = _uiManagerServices.GetPopupInDict<UIAlertDialog>();
            _uiManagerServices.ClosePopupUI(_uiAlertDialog);
            _uiAlertDialog.SetText(DISCONNECTMENTTITLE, DISCONNECTMENTBOBY);
        }

        public void Dispose()
        {
            _registrar.Unregister(this);
        }

        public UniTask HandleDisconnectAsync(
            ulong disconnectID,
            RelayManager relayManager,
            LobbyManager lobbyManager,
            SceneManagerEx sceneManger)
        {
            if (relayManager.NetworkManagerEx.IsHost &&
                relayManager.NetworkManagerEx.LocalClientId != disconnectID)
            {
                relayManager.ChoicePlayerCharactersDict.Remove(disconnectID);
                return UniTask.CompletedTask;
            }

            if (relayManager.NetworkManagerEx.LocalClientId != disconnectID)
                return UniTask.CompletedTask;

            // 전멸 UI가 떠 있는 상태의 disconnect는 의도된 릴레이 종료다.
            // 이 경우 연결 끊김 팝업을 띄우지 않고, 로비 이동도 전멸 UI가 담당한다.
            if ((_uiManagerServices.Try_Get_Scene_UI(out UIAllPlayerDead allPlayerDeadUi) &&
                allPlayerDeadUi.gameObject.activeInHierarchy)
                || (_uiManagerServices.Try_Get_Scene_UI(out UIEnding endingUi) &&
                    endingUi.gameObject.activeInHierarchy))
            {
                return UniTask.CompletedTask;
            }

            if (relayManager.DisconnectCause == RelayDisconnectCause.IntentionalLeaveToLobby)
            {
                MoveToLobbyAsync().Forget();
            }
            else
            {
                _uiAlertDialog.SetCloseButtonOverride(() => MoveToLobbyAsync().Forget());
                _uiManagerServices.ShowPopupUI(_uiAlertDialog);
            }

            async UniTask MoveToLobbyAsync()
            {
                relayManager.ShutDownRelay(RelayDisconnectCause.IntentionalLeaveToLobby);
                await relayManager.WaitForRelayShutdownAsync();

                sceneManger.LoadSceneWithLoadingScreen(Define.SceneName.LobbyScene);
                lobbyManager.InitLobbyScene().Forget();
            }

            return UniTask.CompletedTask;
        }
    }
}
