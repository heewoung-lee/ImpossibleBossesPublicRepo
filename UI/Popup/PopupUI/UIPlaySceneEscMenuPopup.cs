using GameManagers;
using GameManagers.RelayManagement;
using GameManagers.UIManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UIPlaySceneEscMenuPopup : UIPopup
    {
        private IUIManagerServices _uiManagerServices;
        private RelayManager _relayManager;
        private const string HOSTMENT = "정말 로비로 나가시겠어요?\n 호스트가 나가면 모든 클라이언트들이 로비로 나가지게 됩니다.";
        private const string CLIENTMENT = "정말 로비로 나가시겠어요?";
        private UICheckDialog _checkDialog;

        [Inject]
        public void Construct(IUIManagerServices uiManagerServices, RelayManager relayManager)
        {
            _uiManagerServices = uiManagerServices;
            _relayManager = relayManager;
        }

        enum Buttons
        {
            SettingButton,
            ContinueButton,
            LobbyButton,
            ExitButton
        }

        private Canvas _canvas;
        private Button _btnResume;
        private Button _btnSettings;
        private Button _btnLobby;
        private Button _btnQuit;


        protected override void StartInit()
        {
            _checkDialog = _uiManagerServices.GetPopupInDict<UICheckDialog>();
            _uiManagerServices.ClosePopupUI(_checkDialog);
        }

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();

            Bind<Button>(typeof(Buttons));

            // 프리팹 구성에 따라 Find/SerializeField/바인딩 유틸 중 택1
            _btnResume = Get<Button>((int)Buttons.ContinueButton);
            _btnSettings = Get<Button>((int)Buttons.SettingButton);
            _btnLobby = Get<Button>((int)Buttons.LobbyButton);
            _btnQuit = Get<Button>((int)Buttons.ExitButton);

            _btnResume.onClick.AddListener(OnClickResume);
            _btnSettings.onClick.AddListener(OnClickSettings);

            _btnLobby.onClick.AddListener(OnMoveLobby);
            _btnQuit.onClick.AddListener(OnClickQuit);
        }

        public bool IsVisible => _canvas != null && _canvas.enabled;


        private void OnClickResume()
        {
            //어차피 현재 팝업이 탑이므로 ClosePopupUI()로 닫아도 안전
            _uiManagerServices.ClosePopupUI();
        }

        private void OnMoveLobby()
        {
            if (_relayManager.NetworkManagerEx.IsHost)
            {
                _checkDialog.SetText("확인", HOSTMENT);
            }
            else
            {
                _checkDialog.SetText("확인", CLIENTMENT);
            }
            _checkDialog.SetCloseButtonOverride(BacktoLobby);
            _uiManagerServices.ShowPopupUI(_checkDialog);
        }

        private void BacktoLobby()
        {
            _relayManager.ShutDownRelay(RelayDisconnectCause.IntentionalLeaveToLobby);
        }

        private void OnClickSettings()
        {
            var settings = _uiManagerServices.GetPopupUIFromResource<UISettingsPopup>();
            _uiManagerServices.ShowPopupUI(settings);
        }

        private void OnClickQuit()
        {
            _relayManager.ShutDownRelay(RelayDisconnectCause.ApplicationQuit);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}