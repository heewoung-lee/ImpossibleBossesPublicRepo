using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI.Scene.SceneUI
{
    public class UIGameQuit : MonoBehaviour
    {
        private Button _quitButton;

        private void Awake()
        {
            _quitButton = GetComponent<Button>();
            
            _quitButton.onClick.AddListener(Quit);
        }

        private void Quit()
        {
            // 전처리기를 사용하여 에디터와 빌드 버전을 구분
#if UNITY_EDITOR
            // 에디터에서 플레이 모드 중지
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}