using GameManagers;
using GameManagers.UIManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UILobbyEscMenuPopup : UIPopup
    {
        private IUIManagerServices _uiManagerServices;

        [Inject]
        public void Construct(IUIManagerServices uiManagerServices)
        {
            _uiManagerServices = uiManagerServices;
        }

        enum Buttons
        {
            SettingButton,
            ContinueButton,
            ExitButton
        }
        

        private Canvas _canvas;
        private Button _btnResume;
        private Button _btnSettings;
        private Button _btnQuit;

        protected override void StartInit()
        {
        }

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            
            Bind<Button>(typeof(Buttons));

            // 프리팹 구성에 따라 Find/SerializeField/바인딩 유틸 중 택1
            _btnResume = Get<Button>((int)Buttons.ContinueButton);
            _btnSettings = Get<Button>((int)Buttons.SettingButton);
            _btnQuit = Get<Button>((int)Buttons.ExitButton);
            
            _btnResume.onClick.AddListener(OnClickResume);
            _btnSettings.onClick.AddListener(OnClickSettings);
            _btnQuit.onClick.AddListener(OnClickQuit);
        }

        public bool IsVisible => _canvas != null && _canvas.enabled;


        private void OnClickResume()
        {
            //어차피 현재 팝업이 탑이므로 ClosePopupUI()로 닫아도 안전
            _uiManagerServices.ClosePopupUI();
        }

        private void OnClickSettings()
        {
            var settings = _uiManagerServices.GetPopupUIFromResource<UISettingsPopup>();
            _uiManagerServices.ShowPopupUI(settings);
        }

        private void OnClickQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}