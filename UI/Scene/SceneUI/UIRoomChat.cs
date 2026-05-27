using System;
using System.Linq;
using GameManagers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UIRoomChat : UILobbyChat
    {
        private const int ExtensionSize = 400;
        enum AddButtons
        {
            ExtensionButton
        }

        enum RectTransforms
        {
            BackgroundGraphic,
            ChatScrollRect,
            Viewport,
            ChattingLog
        }

        enum ButtonImages
        {
            BackGroundSprite,
            InnerSprite
        }

        Button _extensionButton;
        RectTransform _backGroundGraphic;
        RectTransform _chatScrollRect;
        RectTransform _viewportRect;
        RectTransform _chattingLogRect;
        ScrollRect _roomChatScrollRect;

        Image[] _buttonImages;
        Sprite[] _extensionImage;
        Sprite[] _contractionImage;
        Vector2 _originBackgroundOffsetMax;
        Vector2 _originChatScrollOffsetMax;
        float _originViewportPositionY;
        float _originViewportHeight;
        bool _isChatRectInitialized = false;
        bool _isStateExtenstion = false;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            AddBind<Button>(typeof(AddButtons),out string[] indexString);
            Bind<RectTransform>((typeof(RectTransforms)));
            int extensionButtonIndex = Array.FindIndex(indexString, strings => strings == Enum.GetName(typeof(AddButtons), AddButtons.ExtensionButton));
            _extensionButton = Get<Button>(extensionButtonIndex);
            _backGroundGraphic = Get<RectTransform>((int)RectTransforms.BackgroundGraphic);
            _chatScrollRect = Get<RectTransform>((int)RectTransforms.ChatScrollRect);
            _viewportRect = Get<RectTransform>((int)RectTransforms.Viewport);
            _chattingLogRect = Get<RectTransform>((int)RectTransforms.ChattingLog);
            _roomChatScrollRect = _chatScrollRect.GetComponent<ScrollRect>();
            _originBackgroundOffsetMax = _backGroundGraphic.offsetMax;
            _originChatScrollOffsetMax = _chatScrollRect.offsetMax;
            _extensionButton.onClick.AddListener(SetSwitchingChatting);
            _buttonImages = _extensionButton.gameObject.GetComponentsInChildren<Image>();
            _extensionImage = _extensionButton.gameObject.GetComponentsInChildren<Image>().Select(image => image.sprite).ToArray();
            _contractionImage = new Sprite[_extensionImage.Length];
        }


        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            _contractionImage[(int)ButtonImages.BackGroundSprite] = _resourcesServices.Load<Sprite>("Art/UI/RoomScene/RoomChat/ReductionBtn");
            _contractionImage[(int)ButtonImages.InnerSprite] = _resourcesServices.Load<Sprite>("Art/UI/ButtonImage/Icon_Minus");
        }

        protected override void StartInit()
        {
            base.StartInit();
            InitializeChatRect();
        }

        private void SetSwitchingChatting()
        {
            InitializeChatRect();

            if(_isStateExtenstion == false)
            {
                _backGroundGraphic.offsetMax = _originBackgroundOffsetMax + Vector2.up * ExtensionSize;
                _chatScrollRect.offsetMax = _originChatScrollOffsetMax + Vector2.up * ExtensionSize;
                SetViewportHeight(_originViewportHeight + ExtensionSize);
                _isStateExtenstion = true;
                _buttonImages[(int)ButtonImages.BackGroundSprite].sprite = _contractionImage[(int)ButtonImages.BackGroundSprite];
                _buttonImages[(int)ButtonImages.InnerSprite].sprite = _contractionImage[(int)ButtonImages.InnerSprite];
            }
            else
            {
                _backGroundGraphic.offsetMax = _originBackgroundOffsetMax;
                _chatScrollRect.offsetMax = _originChatScrollOffsetMax;
                SetViewportHeight(_originViewportHeight);
                _isStateExtenstion = false;
                _buttonImages[(int)ButtonImages.BackGroundSprite].sprite = _extensionImage[(int)ButtonImages.BackGroundSprite];
                _buttonImages[(int)ButtonImages.InnerSprite].sprite = _extensionImage[(int)ButtonImages.InnerSprite];
            }

            ScrollRoomChatToBottom();
        }

        private void InitializeChatRect()
        {
            if (_isChatRectInitialized == true)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            _originViewportPositionY = _chatScrollRect.rect.height * _viewportRect.anchorMin.y + _viewportRect.offsetMin.y;
            _originViewportHeight = _viewportRect.rect.height;

            SetViewportHeight(_originViewportHeight);
            SetChattingLogRect();
            ScrollRoomChatToBottom();
            _isChatRectInitialized = true;
        }

        private void SetViewportHeight(float height)
        {
            Vector2 anchorMin = _viewportRect.anchorMin;
            anchorMin.y = 0f;
            _viewportRect.anchorMin = anchorMin;

            Vector2 anchorMax = _viewportRect.anchorMax;
            anchorMax.y = 0f;
            _viewportRect.anchorMax = anchorMax;

            Vector2 pivot = _viewportRect.pivot;
            pivot.y = 0f;
            _viewportRect.pivot = pivot;

            Vector2 anchoredPosition = _viewportRect.anchoredPosition;
            anchoredPosition.y = _originViewportPositionY;
            _viewportRect.anchoredPosition = anchoredPosition;

            _viewportRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void SetChattingLogRect()
        {
            Vector2 anchorMin = _chattingLogRect.anchorMin;
            anchorMin.y = 0f;
            _chattingLogRect.anchorMin = anchorMin;

            Vector2 anchorMax = _chattingLogRect.anchorMax;
            anchorMax.y = 0f;
            _chattingLogRect.anchorMax = anchorMax;

            Vector2 pivot = _chattingLogRect.pivot;
            pivot.y = 0f;
            _chattingLogRect.pivot = pivot;

            Vector2 anchoredPosition = _chattingLogRect.anchoredPosition;
            anchoredPosition.y = 0f;
            _chattingLogRect.anchoredPosition = anchoredPosition;
        }

        private void ScrollRoomChatToBottom()
        {
            Canvas.ForceUpdateCanvases();
            _roomChatScrollRect.verticalNormalizedPosition = 0f;
        }


    }
}
