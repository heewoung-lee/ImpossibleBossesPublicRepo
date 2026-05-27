using System;
using Module.UI_Module;
using TMPro;
using UnityEngine;

namespace UI.SubItem
{
    public class UIMessageErrorToast : UIBase
    {
        enum Texts
        {
            MessageText
        }

        enum GameObjects
        {
            ToastPanel
        }

        private TMP_Text _messageText;
        private GameObject _toastPanel;
        private RectTransform _toastParentRectTransform;
        private RectTransform _toastPanelRectTransform;
        private ModuleUIFadeOut _fadeOutModule;
        private Action _doneFadeoutAction;
        private Vector3 _defaultToastAnchoredPosition;

        protected override void AwakeInit()
        {
            Bind<TMP_Text>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            _messageText = Get<TMP_Text>((int)Texts.MessageText);
            _toastPanel = Get<GameObject>((int)GameObjects.ToastPanel);
            _toastPanelRectTransform = _toastPanel.GetComponent<RectTransform>();
            _toastParentRectTransform = _toastPanelRectTransform.parent as RectTransform;
            _defaultToastAnchoredPosition = _toastPanelRectTransform.anchoredPosition3D;
            _fadeOutModule = _toastPanel.GetComponent<ModuleUIFadeOut>();
            _fadeOutModule.DoneFadeoutEvent += HandleDoneFadeOut;
            ApplyToastOffset(Vector3.zero);
            HideImmediately();
        }

        protected override void StartInit()
        {
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();

            if (Canvas != null)
            {
                Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Canvas.overrideSorting = true;
                Canvas.sortingOrder = 10000;
            }

            ApplyToastOffset(Vector3.zero);
            HideImmediately();
        }

        private void OnDestroy()
        {
            if (_fadeOutModule != null)
            {
                _fadeOutModule.DoneFadeoutEvent -= HandleDoneFadeOut;
            }
        }

        /// <summary>
        /// 기본 위치에서 에러 토스트를 출력합니다.
        /// 사용 예시:
        /// toast.Show("오류가 발생했습니다.");
        /// </summary>
        public void Show(string message, Action doneFadeoutAction = null)
        {
            Show(message, Vector3.zero, doneFadeoutAction);
        }

        /// <summary>
        /// 부모 RectTransform 크기 비율 기준으로 위치 오프셋을 적용한 뒤 에러 토스트를 출력합니다.
        /// x 는 부모 너비 기준 비율(%), y 는 부모 높이 기준 비율(%), z 는 anchoredPosition3D 에 그대로 더해집니다.
        /// 사용 예시:
        /// toast.Show("오류가 발생했습니다.", new Vector3(0, 10, 0));
        /// 위 예시는 현재 기본 위치에서 부모 높이의 10% 만큼 위로 올립니다.
        /// </summary>
        public void Show(string message, Vector3 offset, Action doneFadeoutAction = null)
        {
            _doneFadeoutAction = doneFadeoutAction;
            _messageText.text = message;
            ApplyToastOffset(offset);

            if (_toastPanel.activeSelf == false)
            {
                _toastPanel.SetActive(true);
            }

            _fadeOutModule.PlayFadeOut();
        }

        public void HideImmediately()
        {
            _doneFadeoutAction = null;

            if (_toastPanel != null)
            {
                _fadeOutModule.HideImmediate();
            }
        }

        private void HandleDoneFadeOut()
        {
            Action doneFadeoutAction = _doneFadeoutAction;
            _doneFadeoutAction = null;
            doneFadeoutAction?.Invoke();
        }

        private void ApplyToastOffset(Vector3 offset)
        {
            Vector2 parentSize = _toastParentRectTransform.rect.size;
            // x/y 는 퍼센트 입력값을 부모 RectTransform 실제 크기에 맞는 좌표값으로 변환한다.
            Vector3 scaledOffset = new Vector3(
                parentSize.x * (offset.x * 0.01f),
                parentSize.y * (offset.y * 0.01f),
                offset.z);

            _toastPanelRectTransform.anchoredPosition3D = _defaultToastAnchoredPosition + scaledOffset;
        }
    }
}
