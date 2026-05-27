using System;
using Cysharp.Threading.Tasks;
using GameManagers.LobbyManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UIRoomInventory : UIScene
    {
        [Inject] private LobbyManager _lobbyManager;

        enum Transforms
        {
            RoomContent,
            RoomPanel
        }

        enum Buttons
        {
            PreviousBtn,
            NextBtn
        }

        private Transform _roomContent;
        private Transform _roomPanel;
        private Button _previousButton;
        private Button _nextButton;
        private TMP_Text _currentRoomPageText;
        private bool _isInitialRoomListRefreshRequested;

        public Transform RoomContent => _roomContent;
        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Transform>(typeof(Transforms));
            Bind<Button>(typeof(Buttons));

            _roomContent = Get<Transform>((int)Transforms.RoomContent);
            _roomPanel = Get<Transform>((int)Transforms.RoomPanel);
            _previousButton = Get<Button>((int)Buttons.PreviousBtn);
            _nextButton = Get<Button>((int)Buttons.NextBtn);
            _currentRoomPageText = _roomPanel.GetComponentInChildren<TMP_Text>(true);

            _previousButton.onClick.AddListener(OnClickPreviousButton);
            _nextButton.onClick.AddListener(OnClickNextButton);
        }

        protected override void StartInit()
        {
            base.StartInit();
            RefreshRoomPageState();
            RequestInitialRoomListRefresh();
        }

        public void SetRoomPageState(int currentRoomPage, bool canMovePreviousRoomPage, bool canMoveNextRoomPage)
        {
            _currentRoomPageText.text = currentRoomPage.ToString();
            _previousButton.interactable = canMovePreviousRoomPage;
            _nextButton.interactable = canMoveNextRoomPage;
        }

        private void OnClickPreviousButton()
        {
            MoveRoomPage(_lobbyManager.PreviousRoomPage).Forget();
        }

        private void OnClickNextButton()
        {
            MoveRoomPage(_lobbyManager.NextRoomPage).Forget();
        }

        private async UniTask MoveRoomPage(Func<UniTask> moveRoomPage)
        {
            SetRoomPageButtonsInteractable(false);
            try
            {
                await moveRoomPage();
            }
            finally
            {
                RefreshRoomPageState();
            }
        }

        private void RefreshRoomPageState()
        {
            SetRoomPageState(
                _lobbyManager.CurrentRoomPage,
                _lobbyManager.CanMovePreviousRoomPage,
                _lobbyManager.CanMoveNextRoomPage);
        }

        private void RequestInitialRoomListRefresh()
        {
            if (_lobbyManager.IsDoneLobbyInitEvent)
            {
                RefreshInitialRoomList().Forget();
                return;
            }

            _lobbyManager.InitDoneEvent -= OnLobbyInitDone;
            _lobbyManager.InitDoneEvent += OnLobbyInitDone;
        }

        private void OnLobbyInitDone()
        {
            _lobbyManager.InitDoneEvent -= OnLobbyInitDone;
            RefreshInitialRoomList().Forget();
        }

        private async UniTask RefreshInitialRoomList()
        {
            if (_isInitialRoomListRefreshRequested)
                return;

            _isInitialRoomListRefreshRequested = true;
            SetRoomPageButtonsInteractable(false);
            try
            {
                await _lobbyManager.ReFreshRoomList();
            }
            finally
            {
                RefreshRoomPageState();
            }
        }

        private void SetRoomPageButtonsInteractable(bool isInteractable)
        {
            _previousButton.interactable = isInteractable;
            _nextButton.interactable = isInteractable;
        }

        private void OnDestroy()
        {
            if (_lobbyManager != null)
                _lobbyManager.InitDoneEvent -= OnLobbyInitDone;

            if (_previousButton != null)
                _previousButton.onClick.RemoveListener(OnClickPreviousButton);

            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(OnClickNextButton);
        }
    }
}
