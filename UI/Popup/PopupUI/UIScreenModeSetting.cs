using System;
using UI.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup.PopupUI
{
    public class UIScreenModeSetting : UIBase
    {
        enum ScreenMode
        {
            FullScreenMode,
            WindowMode        
        }


        enum DifferenceHierarchyButton
        {
            ApplyButton
        }
        
        private Toggle _fullScreenToggle;
        private Toggle _windowedToggle;
        
        private Button _applyButton;

        private const string PREF_FULLSCREEN = "IsFullScreen";
        
        private bool _targetIsFullScreen;

        protected override void AwakeInit()
        {
            Bind<Toggle>(typeof(ScreenMode));
            
            _fullScreenToggle = Get<Toggle>((int)ScreenMode.FullScreenMode);
            _windowedToggle = Get<Toggle>((int)ScreenMode.WindowMode);
        }

        protected override void StartInit()
        {
            InitState();

            _fullScreenToggle.onValueChanged.AddListener((isOn) => 
            {
                if (isOn) _targetIsFullScreen = true;
            });

            _windowedToggle.onValueChanged.AddListener((isOn) => 
            {
                if (isOn) _targetIsFullScreen = false;
            });
            
            AddBind<Button>(typeof(DifferenceHierarchyButton),out string[] bindIndexes,transform.parent.parent);
            int extensionButtonIndex = Array.FindIndex(bindIndexes, strings => strings == Enum.GetName(typeof(DifferenceHierarchyButton), DifferenceHierarchyButton.ApplyButton));
            _applyButton = Get<Button>(extensionButtonIndex); //다른 계층에 있는 바인드된 오브젝트를 가져와야 하므로 Start
            _applyButton.onClick.AddListener(ApplyScreenMode);
        }
        
        private void OnEnable()
        {
            // StartInit()이 실행된 이후라면 UI 갱신
            if (_fullScreenToggle != null)
            {
                InitState();
            }
        }

        private void InitState()
        {
            // 현재 실제 화면 상태 가져오기
            _targetIsFullScreen = Screen.fullScreen;

            // UI 갱신 (이벤트 트리거 방지를 위해 SetIsOnWithoutNotify 사용 권장)
            _fullScreenToggle.SetIsOnWithoutNotify(_targetIsFullScreen);
            _windowedToggle.SetIsOnWithoutNotify(!_targetIsFullScreen);
        }

        public void ApplyScreenMode()
        {
            if (Screen.fullScreen != _targetIsFullScreen)
            {
                Screen.fullScreen = _targetIsFullScreen;
                
                PlayerPrefs.SetInt(PREF_FULLSCREEN, _targetIsFullScreen ? 1 : 0);
                PlayerPrefs.Save();
                
                Debug.Log($"화면 모드 변경 완료: {(_targetIsFullScreen ? "전체화면" : "창모드")}");
            }
        }
    }
}