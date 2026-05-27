using System;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.SoundManagement;
using GameManagers.VivoxManagement;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UILobbyChat : UIScene
    {
        [Inject] private ISendMessage _sendMessage;
        [Inject] private IVivoxSession _vivoxSession;

        enum Buttons
        {
            SendButton
        }

        enum InputFields
        {
            ChattingInputField
        }

        enum Texts
        {
            ChattingLog
        }

        enum ScrollRects
        {
            ChatScrollRect
        }

        private Button _sendButton;
        private TMP_InputField _chattingInputField;
        private TMP_Text _chatLog;
        private ScrollRect _chattingScrollRect;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Button>(typeof(Buttons));
            Bind<TMP_Text>(typeof(Texts));
            Bind<TMP_InputField>(typeof(InputFields));
            Bind<ScrollRect>(typeof(ScrollRects));
            _sendButton = Get<Button>((int)Buttons.SendButton);
            _chattingInputField = Get<TMP_InputField>((int)InputFields.ChattingInputField);
            _chatLog = Get<TMP_Text>((int)Texts.ChattingLog);
            _chattingScrollRect = Get<ScrollRect>((int)ScrollRects.ChatScrollRect);
            _sendButton.onClick.AddListener(() =>
            {
                SendChatingMessage(_chattingInputField.text).Forget();
            });
            _chattingInputField.onSubmit.AddListener(HandleSubmitChatting);
            _sendButton.interactable = false;
        }

        public void SendText(string text)
        {
            _chatLog.text += text;
            _chatLog.text += "\n";
        }

        protected override void StartInit()
        {
            base.StartInit();
            InitButtonInteractable();
            VivoxService.Instance.ChannelMessageReceived += ChannelMessageReceived;
        }

        private void InitButtonInteractable()
        {
            if (_vivoxSession.CheckDoneLoginProcess == false)
            {
                _vivoxSession.VivoxDoneLoginEvent += ButtonInteractable;
            }
            else
            {
                ButtonInteractable();
            }
        }

        private void OnDestroy()
        {
            if (VivoxService.Instance != null)
            {
                VivoxService.Instance.ChannelMessageReceived -= ChannelMessageReceived;
            }

            if (_vivoxSession != null)
            {
                _vivoxSession.VivoxDoneLoginEvent -= ButtonInteractable;
            }
        }

        public async UniTaskVoid SendChatingMessage(string message)
        {
            if (string.IsNullOrEmpty(_chattingInputField.text) || _sendButton.interactable == false)
                return;

            try
            {
                string tempmessage = message;
                InitializeChattingField();

                await _sendMessage.SendMessageAsync(tempmessage);
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"Error sending message: {ex.Message}");
            }
            finally
            {
                InitializeChattingField();
            }


            void InitializeChattingField()
            {
                _chattingScrollRect.verticalNormalizedPosition = 0f;
                _chattingInputField.text = "";
                _chattingInputField.Select();
                _chattingInputField.ActivateInputField();
            }
           
        }

        private void HandleSubmitChatting(string text)
        {
            if (string.IsNullOrEmpty(_chattingInputField.text) || _sendButton.interactable == false)
            {
                return;
            }

            _soundManagerServices.PlayUiSfx(gameObject, UICommonSoundCueId.Click);
            SendChatingMessage(text).Forget();
        }

        private void ChannelMessageReceived(VivoxMessage message)
        {
            if (message.ChannelName != _vivoxSession.CurrentChannel)
                return;

            string messageText = message.MessageText;
            _chatLog.text += $"{messageText} \n";
        }

        private void ButtonInteractable()
        {
            _sendButton.interactable = true;
        }
    }
}
