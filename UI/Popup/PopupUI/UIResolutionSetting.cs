using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // 버튼 제어를 위해 추가

namespace UI.Popup.PopupUI
{
    public class UIResolutionSetting : UIBase
    {
        private TMP_Text _resolutionText; 
        private Button _prevButton;
        private Button _nextButton; 
        private Button _applyButton; 

        private List<Resolution> _uniqueResolutions = new List<Resolution>();
        
        // _currentIndex: 현재 '화면'에 보여지고 있는 해상도 (선택 중인 것)
        private int _currentIndex = 0;

        enum Buttons
        {
            PreButton,
            NextButton,
        }

        enum DifferenceHierarchyButton
        {
            ApplyButton
        }

        enum Texts
        {
            ResolutionText
        }

        protected override void AwakeInit()
        {
            Bind<Button>(typeof(Buttons));
            Bind<TMP_Text>(typeof(Texts));
            
            _resolutionText = Get<TMP_Text>((int)Texts.ResolutionText);
            _prevButton = Get<Button>((int)Buttons.PreButton);
            _nextButton = Get<Button>((int)Buttons.NextButton);
            
          
            _prevButton.onClick.AddListener(() => ChangeIndex(-1));
            _nextButton.onClick.AddListener(() => ChangeIndex(1));
            
            InitResolutions();
        }
        
        protected override void StartInit()
        {
             AddBind<Button>(typeof(DifferenceHierarchyButton),out string[] indexString,transform.parent.parent);
            int extensionButtonIndex = Array.FindIndex(indexString, strings => strings == Enum.GetName(typeof(DifferenceHierarchyButton), DifferenceHierarchyButton.ApplyButton));
            _applyButton = Get<Button>(extensionButtonIndex);
            _applyButton.onClick.AddListener(ApplyResolution);
        }

   

        private void OnEnable()
        {
            // 중요: 창이 켜질 때마다 현재 실제 해상도로 인덱스를 초기화해줘야 함.
            // 그래야 이전에 만지작거리다가 '취소'하고 나간 상태가 리셋됨.
            FindCurrentResolutionIndex();
            UpdateUI();
        }

        private void InitResolutions()
        {
            _uniqueResolutions.Clear();
            Resolution[] allResolutions = Screen.resolutions;

            // 16:9 비율만 필터링 (기존 로직 유지)
            float targetRatio = 16.0f / 9.0f;

            foreach (Resolution item in allResolutions)
            {
                float ratio = (float)item.width / item.height;

                if (Mathf.Abs(ratio - targetRatio) < 0.01f)
                {
                    // 중복 제거 (기존 로직 유지)
                    if (_uniqueResolutions.Exists(x => x.width == item.width && x.height == item.height))
                        continue;

                    _uniqueResolutions.Add(item);
                }
            }
        }

        // 현재 실행 중인 게임의 해상도가 리스트의 몇 번째인지 찾음
        private void FindCurrentResolutionIndex()
        {
            // 기본값 0
            _currentIndex = 0; 
            
            for (int i = 0; i < _uniqueResolutions.Count; i++)
            {
                if (_uniqueResolutions[i].width == Screen.width && 
                    _uniqueResolutions[i].height == Screen.height)
                {
                    _currentIndex = i;
                    return;
                }
            }
        }

        // 화살표 눌렀을 때 실행 (좌:-1, 우:+1)
        private void ChangeIndex(int direction)
        {
            _currentIndex += direction;

            // 인덱스 범위 순환 (마지막에서 오른쪽 누르면 처음으로)
            if (_currentIndex >= _uniqueResolutions.Count) 
                _currentIndex = 0;
            else if (_currentIndex < 0) 
                _currentIndex = _uniqueResolutions.Count - 1;

            UpdateUI();
        }

        // 텍스트 UI만 업데이트 (실제 해상도는 안 바뀜!)
        private void UpdateUI()
        {
            if (_uniqueResolutions.Count == 0) return;

            Resolution res = _uniqueResolutions[_currentIndex];
            _resolutionText.text = $"{res.width} x {res.height}";
        }

        // [확인] 버튼을 눌렀을 때만 진짜로 바뀜
        public void ApplyResolution()
        {
            if (_uniqueResolutions.Count == 0) return;

            Resolution target = _uniqueResolutions[_currentIndex];
            
            // 전체화면 모드 유지하면서 해상도 변경
            Screen.SetResolution(target.width, target.height, Screen.fullScreenMode);
        }
    }
}