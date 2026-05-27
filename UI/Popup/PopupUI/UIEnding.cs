using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameManagers.InputManagement;
using GameManagers.LobbyManagement;
using GameManagers.RelayManagement;
using GameManagers.SceneManagement;
using TMPro;
using UI.Scene;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UIEnding : UIScene
    {
        private const int AutoMoveToLobbyDelaySeconds = 30;
        private const float ScreenDimmedDuration = 1.6f;
        private const float ScreenDimmedTargetAlpha = 241f / 255f;
        private const string VictoryBgmCueId = "VictoryBGM";
        private const string EndingMessageFormat = "축하합니다. 토벌에 성공하셨습니다.\n{0}초후 로비로 이동합니다.";

        private LobbyManager _lobbyManager;
        private IInputAsset _inputAsset;
        private RelayManager _relayManager;
        private SceneManagerEx _sceneManagerEx;
        private InputActionMap[] _disabledActionMaps;

        enum Buttons
        {
            ButtonMoveLobby
        }

        enum Images
        {
            ScreenDimmed
        }

        enum Texts
        {
            TextEnding
        }

        enum Objects
        {
            Result,
            TextEnding,
            ButtonMoveLobby
        }

        private Button _moveToLobbyButton;
        private Image _screenDimmedImage;
        private TMP_Text _endingText;
        private GameObject[] _revealObjects;
        private CancellationTokenSource _autoMoveLobbyCts;
        private bool _isMoveLobbyRequested;

        [Inject]
        public void Construct(
            LobbyManager lobbyManager,
            RelayManager relayManager,
            SceneManagerEx sceneManagerEx,
            IInputAsset inputAsset)
        {
            _lobbyManager = lobbyManager;
            _relayManager = relayManager;
            _sceneManagerEx = sceneManagerEx;
            _inputAsset = inputAsset;
        }

        protected override void AwakeInit()
        {
            Bind<Button>(typeof(Buttons));
            Bind<Image>(typeof(Images));
            Bind<TMP_Text>(typeof(Texts));
            Bind<GameObject>(typeof(Objects));

            _moveToLobbyButton = Get<Button>((int)Buttons.ButtonMoveLobby);
            _screenDimmedImage = Get<Image>((int)Images.ScreenDimmed);
            _endingText = Get<TMP_Text>((int)Texts.TextEnding);
            _revealObjects = new[]
            {
                GetObject((int)Objects.Result),
                GetObject((int)Objects.TextEnding),
                GetObject((int)Objects.ButtonMoveLobby)
            };

            for (int i = 0; i < _revealObjects.Length; i++)
            {
                _revealObjects[i].SetActive(false);
            }

            _moveToLobbyButton.onClick.AddListener(MoveToLobby);
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _soundManagerServices.StopBgm();
            _soundManagerServices.PlayUiSfx(gameObject, VictoryBgmCueId);
            SetInputBlocked(true);
            _isMoveLobbyRequested = false;
            _autoMoveLobbyCts?.Cancel();
            _autoMoveLobbyCts?.Dispose();
            _autoMoveLobbyCts = new CancellationTokenSource();
            SetRevealObjectsActive(false);
            SetScreenDimmedAlpha(0f);
            UpdateEndingText(AutoMoveToLobbyDelaySeconds);
            PlayRevealAnimationAsync(_autoMoveLobbyCts.Token).Forget();
            AutoMoveToLobbyAsync(_autoMoveLobbyCts.Token).Forget();
        }

        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            SetInputBlocked(false);
            _autoMoveLobbyCts?.Cancel();
            _autoMoveLobbyCts?.Dispose();
            _autoMoveLobbyCts = null;
        }

        private void SetInputBlocked(bool isBlocked)
        {
            if (isBlocked)
            {
                InputActionAsset actionAsset = _inputAsset.GetInputActionAsset();
                _disabledActionMaps = new[]
                {
                    actionAsset.FindActionMap(Define.ControllerType.Player.ToString(), false),
                    actionAsset.FindActionMap(Define.ControllerType.Camera.ToString(), false),
                    actionAsset.FindActionMap(Define.ControllerType.UI.ToString(), false)
                };

                foreach (InputActionMap actionMap in _disabledActionMaps)
                {
                    actionMap?.Disable();
                }

                return;
            }

            if (_disabledActionMaps == null)
            {
                return;
            }

            foreach (InputActionMap actionMap in _disabledActionMaps)
            {
                actionMap?.Enable();
            }

            _disabledActionMaps = null;
        }

        private void MoveToLobby()
        {
            if (_isMoveLobbyRequested)
            {
                return;
            }

            _isMoveLobbyRequested = true;
            _autoMoveLobbyCts?.Cancel();
            MoveToLobbyAsync().Forget();
        }

        private async UniTaskVoid AutoMoveToLobbyAsync(CancellationToken cancellationToken)
        {
            try
            {
                float moveToLobbyAt = Time.realtimeSinceStartup + AutoMoveToLobbyDelaySeconds;
                int previousRemainingSeconds = -1;

                while (true)
                {
                    float remainingTime = moveToLobbyAt - Time.realtimeSinceStartup;
                    int remainingSeconds = Mathf.Max(0, Mathf.CeilToInt(remainingTime));

                    if (remainingSeconds != previousRemainingSeconds)
                    {
                        previousRemainingSeconds = remainingSeconds;
                        UpdateEndingText(remainingSeconds);
                    }

                    if (remainingTime <= 0f)
                    {
                        break;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                MoveToLobby();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTaskVoid PlayRevealAnimationAsync(CancellationToken cancellationToken)
        {
            float elapsedTime = 0f;

            try
            {
                while (elapsedTime < ScreenDimmedDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float alpha = Mathf.Lerp(0f, ScreenDimmedTargetAlpha,
                        Mathf.Clamp01(elapsedTime / ScreenDimmedDuration));
                    SetScreenDimmedAlpha(alpha);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            SetScreenDimmedAlpha(ScreenDimmedTargetAlpha);
            SetRevealObjectsActive(true);
        }

        private async UniTaskVoid MoveToLobbyAsync()
        {
            _relayManager.ShutDownRelay(RelayDisconnectCause.IntentionalLeaveToLobby);
            await _relayManager.WaitForRelayShutdownAsync();
            _sceneManagerEx.LoadSceneWithLoadingScreen(Define.SceneName.LobbyScene);
            _lobbyManager.InitLobbyScene().Forget();
        }

        private void SetScreenDimmedAlpha(float alpha)
        {
            Color dimmedColor = _screenDimmedImage.color;
            dimmedColor.a = alpha;
            _screenDimmedImage.color = dimmedColor;
        }

        private void SetRevealObjectsActive(bool isActive)
        {
            foreach (GameObject revealObject in _revealObjects)
            {
                revealObject.SetActive(isActive);
            }
        }

        private void UpdateEndingText(int remainingSeconds)
        {
            _endingText.text = string.Format(EndingMessageFormat, remainingSeconds);
        }

        protected override void StartInit()
        {
        }
    }
}
