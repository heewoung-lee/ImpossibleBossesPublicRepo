using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.LobbyManagement;
using GameManagers.RelayManagement;
using GameManagers.SceneManagement;
using GameManagers.UIManagement;
using Module.UI_Module;
using NetWork.NGO;
using NetWork.NGO.UI;


using TMPro;
using UI.Popup.PopupUI;
using UI.SubItem;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Scene.SceneUI
{
    struct ReadyButtonImages
    {
        public Sprite ReadyButtonImage;
        public string ReadyButtonText;
        public Color ReadyButtonTextColor;
    }

    public class UIRoomCharacterSelect : UIScene
    {
        private LobbyManager _lobbyManager;
        private SceneManagerEx _sceneManagerEx;
        private RelayManager _relayManager;
        private IUIManagerServices _uiManagerServices;

        [Inject]
        public void Construct(
            LobbyManager lobbyManager,
            SceneManagerEx sceneManagerEx,
            RelayManager relayManager,
            IUIManagerServices uiManagerServices
        )
        {
            _lobbyManager = lobbyManager;
            _sceneManagerEx = sceneManagerEx;
            _relayManager = relayManager;
            _uiManagerServices = uiManagerServices;
        }

        private const int MaxPlayerCount = 8;
        private const string HostMigrationLoadingTitle = "파티장 변경중";
        private const string HostMigrationLoadingBody = "파티장이 나가 새 파티장을 추대하는중 입니다. 잠시만 기다려 주세요.";
        private const string BackToLobbyLoadingTitle = "로비로 이동합니다.";
        private const string BackToLobbyLoadingBody = "로비로 이동중";
        private const int RoomEntryWaitCheckIntervalMs = 1000;
        private const int RoomEntryWaitTimeoutMs = 10000;
        private const string RoomEntryLoadingTitle = "방 정리중";
        private const string RoomEntryLoadingBody = "방을 정리하고 있습니다.";
        private const string RoomEntryFailedTitle = "접속 오류";
        private const string RoomEntryFailedBody = "방에 오류가 생겼습니다.\n로비로 돌아갑니다.";

        enum ReadyButtonStateEnum
        {
            CancelState,
            TrueState
        }

        enum Transforms
        {
            CharactorSelectTr
        }

        enum GameObjects
        {
            LoadingPanel
        }

        enum Buttons
        {
            BackToLobbyButton,
            ButtonReady,
            ButtonStart
        }


        private ChooseCharacterCameraController _chooseCameraController;
        private Transform _charactorSelect;
        private UIRoomPlayerFrame[] _uiRoomPlayerFrames;
        private Button _backToLobbyButton;
        private GameObject _loadingPanel;
        private GameObject _uiCharactorSelectRoot;
        private NetworkManager _netWorkManager;
        private Button _buttonReady;
        private Button _buttonStart;

        private TMP_Text _buttonText;
        private UILoadingProgress _uiLoadingProgress;

        //private CharacterSelectorNgo _characterSelectorNgo;
        private bool _readyButtonState;
        private Transform _ngoUIRootCharacterSelect;
        private string _joincodeCache;
        private Action<ulong> _spawnCharacterSelectEvent;
        private ReadyButtonImages[] _readyButtonStateValue;
        private CharacterSelectorNgo _characterSelectorNgo;
        private bool _isRoomEntryWaitRunning;
        private bool _isRoomEntryFailureHandled;

        private bool _isHostButtonTrigger = false; //start버튼이 중복으로 눌려서 오작동을 막기위한 트리거 장치

        public Transform NgoUIRootCharacterSelect => _ngoUIRootCharacterSelect;
        public bool IsReadyForHostMigration => _characterSelectorNgo != null;

        public ChooseCharacterCameraController ChooseCameraController
        {
            get => _chooseCameraController;
        }

        private UILoadingProgress LoadingProgress
        {
            get
            {
                if (_uiLoadingProgress == null)
                {
                    _uiLoadingProgress = _uiManagerServices.GetOrCreateSceneUI<UILoadingProgress>();
                }

                return _uiLoadingProgress;
            }
        }

        public event Action<ulong> SpawnCharacterSelectEvent
        {
            add { UniqueEventRegister.AddSingleEvent(ref _spawnCharacterSelectEvent, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _spawnCharacterSelectEvent, value); }
        }


        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Transform>(typeof(Transforms));
            Bind<Button>(typeof(Buttons));
            Bind<GameObject>(typeof(GameObjects));

            _charactorSelect = Get<Transform>((int)Transforms.CharactorSelectTr);
            _backToLobbyButton = Get<Button>((int)Buttons.BackToLobbyButton);
            _buttonStart = Get<Button>((int)Buttons.ButtonStart);
            _buttonStart.onClick.AddListener(LoadScenePlayGames);
            _buttonStart.gameObject.SetActive(false);
            _backToLobbyButton.onClick.AddListener(BacktoLobby);
            _loadingPanel = Get<GameObject>((int)GameObjects.LoadingPanel);

            _uiRoomPlayerFrames = new UIRoomPlayerFrame[MaxPlayerCount];
            _buttonReady = Get<Button>((int)Buttons.ButtonReady);
            _buttonText = _buttonReady.GetComponentInChildren<TMP_Text>();
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            _chooseCameraController = _resourcesServices.InstantiateByKey("Prefabs/Map/LobbyScene/ChoosePlayer")
                .GetComponent<ChooseCharacterCameraController>();


            _uiRoomPlayerFrames = _charactorSelect.GetComponentsInChildren<UIRoomPlayerFrame>();

            _netWorkManager = _relayManager.NetworkManagerEx;
            ReadyButtonInitialize();
        }
        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _loadingPanel.SetActive(false);
            _isHostButtonTrigger = false;
        }


        public void Set_NGO_UI_Root_Character_Select(Transform chracterRootTr)
        {
            _ngoUIRootCharacterSelect = chracterRootTr;
            if (_ngoUIRootCharacterSelect.TryGetComponent(out NetworkObject rootCharacterSelect))
            {
                _spawnCharacterSelectEvent?.Invoke(rootCharacterSelect.OwnerClientId);
            }
            else
            {
                Debug.Assert(false, $"{chracterRootTr.name} is not a network object");
            }
        }

        public void SetCharacterSelectorNgo(CharacterSelectorNgo characterSelectorNgo)
        {
            _characterSelectorNgo = characterSelectorNgo;
            bool wasRoomEntryWaitRunning = _isRoomEntryWaitRunning;
            _isRoomEntryWaitRunning = false;

            if (wasRoomEntryWaitRunning && _uiLoadingProgress != null)
            {
                _uiLoadingProgress.HideLoading();
            }
        }

        private void ReadyButtonInitialize()
        {
            _readyButtonStateValue = new ReadyButtonImages[Enum.GetValues(typeof(ReadyButtonStateEnum)).Length];

            _readyButtonStateValue[(int)ReadyButtonStateEnum.TrueState].ReadyButtonImage =
                _buttonReady.GetComponent<Image>().sprite;
            _readyButtonStateValue[(int)ReadyButtonStateEnum.TrueState].ReadyButtonText =
                _buttonReady.GetComponentInChildren<TMP_Text>().text;
            _readyButtonStateValue[(int)ReadyButtonStateEnum.TrueState].ReadyButtonTextColor =
                _buttonReady.GetComponentInChildren<TMP_Text>().color;

            _readyButtonStateValue[(int)ReadyButtonStateEnum.CancelState].ReadyButtonImage =
                _resourcesServices.Load<Sprite>("Art/UI/RoomScene/cancelReadyBtn");
            _readyButtonStateValue[(int)ReadyButtonStateEnum.CancelState].ReadyButtonText = "Not Ready";
            _readyButtonStateValue[(int)ReadyButtonStateEnum.CancelState].ReadyButtonTextColor = Color.white;
        }

  
        private void SubScribeRelayCallback()
        {
            _netWorkManager.OnClientConnectedCallback += EntetedPlayerinLobby;
            _netWorkManager.OnClientDisconnectCallback += DisConnetedPlayerinLobby;
        }

        private void UnscribeRelayCallback()
        {
            _netWorkManager.OnClientConnectedCallback -= EntetedPlayerinLobby;
            _netWorkManager.OnClientDisconnectCallback -= DisConnetedPlayerinLobby;
        }

        public void SetButtonEvent(UnityAction action)
        {
            _buttonReady.onClick.AddListener(action);
        }

        public Transform GetPlayerFrameTransform(int index)
        {
            if (index >= 0 && index < _uiRoomPlayerFrames.Length)
            {
                return _uiRoomPlayerFrames[index].transform;
            }

            return null;
        }

        private void DisConnetedPlayerinLobby(ulong playerIndex)
        {
            UtilDebug.Log("플레이어가 나갔습니다.");

            if (_netWorkManager.IsHost && _ngoUIRootCharacterSelect != null)
            {
                _ngoUIRootCharacterSelect.GetComponent<NgoUIRootCharacterSelect>().LeaveSlot(playerIndex);
                //호스트가 참여자의 인덱스를 수거
            }

            IsCheckAllReadyToPlayers(playerIndex);
        }

        public void IsCheckAllReadyToPlayers(ulong playerIndex = ulong.MaxValue)
        {
            foreach (CharacterSelectorNgo playerNgo in _relayManager.NgoRootUI
                         .GetComponentsInChildren<CharacterSelectorNgo>())
            {
                if (playerNgo.GetComponent<NetworkObject>().OwnerClientId == playerIndex)
                    continue;

                if (playerNgo.IsOwnedByServer)
                    continue;

                if (playerNgo.IsReady == false)
                {
                    SetHostStartButton(false);
                    return;
                }
            }

            SetHostStartButton(true);
        }

        public void EntetedPlayerinLobby(ulong playerIndex)
        {
            SetHostStartButton(false);
            SpawnCharacterSelector(playerIndex);
        }

        public void BacktoLobby()
        {
            LoadingProgress.ShowLoading(BackToLobbyLoadingTitle, BackToLobbyLoadingBody);
            _relayManager.ShutDownRelay(RelayDisconnectCause.IntentionalLeaveToLobby);
        }

        private void OnDestroy()
        {
            UnscribeRelayCallback();
            if (_lobbyManager != null)
            {
                _lobbyManager.HostChangeEvent -= OnHostMigrationEvent;
                _lobbyManager.LobbyLoadingEvent -= OnHostMigrationLoading;
            }
        }

        public void SetHostButton()
        {
            _buttonReady.gameObject.SetActive(false);
            _buttonStart.gameObject.SetActive(true);
        }

        public void SetHostStartButton(bool startButtonstate)
        {
            _buttonStart.interactable = startButtonstate;
        }

        protected override void StartInit()
        {
            base.StartInit();
            _isRoomEntryFailureHandled = false;
            _lobbyManager.LobbyLoadingEvent += OnHostMigrationLoading;
            InitializeCharacterSelectionAsHost();
            _lobbyManager.HostChangeEvent += OnHostMigrationEvent;
            WaitForRoomEntryReadyIfNeededAsync().Forget();
        }

        private void OnHostMigrationLoading(bool isLoading)
        {
            if (_isRoomEntryWaitRunning)
                return;

            if (isLoading)
            {
                LoadingProgress.ShowLoading(HostMigrationLoadingTitle, HostMigrationLoadingBody);
                return;
            }

            if (_uiLoadingProgress != null)
            {
                _uiLoadingProgress.HideLoading();
            }
        }

        private void OnHostMigrationEvent()
        {
            InitializeCharacterSelectionAsHost();
        }

        private async UniTaskVoid WaitForRoomEntryReadyIfNeededAsync()
        {
            if (_relayManager.NetworkManagerEx.IsHost)
                return;

            if (_characterSelectorNgo != null)
                return;

            if (_isRoomEntryWaitRunning)
                return;

            _isRoomEntryWaitRunning = true;
            LoadingProgress.ShowLoading(RoomEntryLoadingTitle, RoomEntryLoadingBody);

            try
            {
                int waitedMs = 0;
                while (waitedMs < RoomEntryWaitTimeoutMs)
                {
                    if (_isRoomEntryFailureHandled)
                        return;

                    if (_characterSelectorNgo != null)
                    {
                        HideRoomEntryLoading();
                        return;
                    }

                    await UniTask.Delay(RoomEntryWaitCheckIntervalMs, ignoreTimeScale: true,
                        cancellationToken: this.GetCancellationTokenOnDestroy());
                    waitedMs += RoomEntryWaitCheckIntervalMs;
                }

                if (_characterSelectorNgo != null)
                {
                    HideRoomEntryLoading();
                    return;
                }

                ShowRoomEntryFailedDialog();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _isRoomEntryWaitRunning = false;
            }
        }

        private void HideRoomEntryLoading()
        {
            if (_uiLoadingProgress != null)
            {
                _uiLoadingProgress.HideLoading();
            }
        }

        public void ShowRoomEntryFailedDialog()
        {
            HideRoomEntryLoading();

            if (_isRoomEntryFailureHandled)
                return;

            _isRoomEntryFailureHandled = true;
            _isRoomEntryWaitRunning = false;

            if (_uiManagerServices.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == false)
            {
                MoveToLobbyAfterRoomEntryFailedAsync().Forget();
                return;
            }

            Canvas canvas = dialog.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 100;
            }

            dialog.AfterAlertEvent(() => MoveToLobbyAfterRoomEntryFailedAsync().Forget())
                .AlertSetText(RoomEntryFailedTitle, RoomEntryFailedBody);
        }

        private async UniTaskVoid MoveToLobbyAfterRoomEntryFailedAsync()
        {
            LoadingProgress.ShowLoading(BackToLobbyLoadingTitle, BackToLobbyLoadingBody);
            _relayManager.ShutDownRelay(RelayDisconnectCause.IntentionalLeaveToLobby);
            _sceneManagerEx.LoadSceneWithLoadingScreen(Define.SceneName.LobbyScene);
            await _lobbyManager.TryJoinLobbyByNameOrCreateWaitLobby();
        }

        private void InitializeCharacterSelectionAsHost()
        {
            if (_relayManager.NetworkManagerEx.IsHost == false)
                return;

            //NgoUIRootCharacterSelect characterSelect = _ngoUIRootCharacterSelectFactory.Create(); 7.23일 팩토리 방식에서 일반 생성으로 변경
            //일반 생성으로 보이나 팩토리로 생성되며 생성 구별은 뒷단에 숨겨놓음
            NgoUIRootCharacterSelect characterSelect = _resourcesServices
                .InstantiateByKey("Prefabs/NGO/NGOUIRootChracterSelect").GetComponent<NgoUIRootCharacterSelect>();
            _relayManager.SpawnNetworkObj(characterSelect.gameObject, parent: _relayManager.NgoRootUI.transform);
            SpawnCharacterSelector(_netWorkManager.LocalClientId); //로컬 아이디로 생성
            SubScribeRelayCallback();
        }


        //2.24일 수정 호스트가 스폰과 부모의 위치만 잡아주고 이후 로직은 
        //CharacterSelectorNgo에서 직접하도록 수정
        private void SpawnCharacterSelector(ulong playerIndex)
        {
            if (_netWorkManager.IsHost) // 호스트가 스폰만 담당하고 포지션 및 크기는 로컬이 담당하도록 분리.
            {
                CharacterSelectorNgo selector = _resourcesServices
                    .InstantiateByKey("Prefabs/NGO/NGOUICharacterSelectRect").GetComponent<CharacterSelectorNgo>();
                //_characterSelectorNgo = _characterSelectorNgoFactory.Create(); 7.23일 
                //SetPositionCharacterSelector(selector.gameObject, playerIndex);

                //여기는 호스트가 스폰 그리고 인덱스 할당만 담당해야함.
                GameObject characterRect =
                    _relayManager.SpawnNetworkObjInjectionOwner(playerIndex, selector.gameObject, destroyOption: true);

                if (_ngoUIRootCharacterSelect == null)
                {
                    SpawnCharacterSelectEvent += SetFrameIndex;
                    SpawnCharacterSelectEvent += (clientID) => { SetParent(); };
                }
                else
                {
                    SetFrameIndex(playerIndex);
                    SetParent();
                }

                void SetFrameIndex(ulong clientID)
                {
                    int playerSlotIndex = _ngoUIRootCharacterSelect.GetComponent<NgoUIRootCharacterSelect>()
                        .AllocateSlot(clientID);
                    selector.PlayerIndex = playerSlotIndex;

                    SpawnCharacterSelectEvent -= SetFrameIndex;
                }

                void SetParent()
                {
                    characterRect.transform.SetParent(_ngoUIRootCharacterSelect, false);

                    SpawnCharacterSelectEvent -= SetFrameIndex;
                }
            }
        }

        public void LoadScenePlayGames() //호스트가 Start버튼을 클릭했을때
        {
            LoadScenePlayGamesAsync().Forget();
        }

        private async UniTask LoadScenePlayGamesAsync()
        {
            if (_isHostButtonTrigger == true) return;
            
            _isHostButtonTrigger = true; // 여기 들어오는 순간 트리거를 true로 해 이후 중복버튼누름을 방지

            _relayManager.NgoRPCCaller.SetGameStartLoadingRpc(true);
            try
            {
                bool isLobbyClosed = await _lobbyManager.CloseCurrentRoomLobbyForGameStartAsync();
                if (isLobbyClosed == false)
                {
                    _relayManager.NgoRPCCaller.SetGameStartLoadingRpc(false);
                    _isHostButtonTrigger = false;
                    return;
                }
                
                _netWorkManager.NetworkConfig.EnableSceneManagement = true;
                _relayManager.RegisterSelectedCharacter(_relayManager.NetworkManagerEx.LocalClientId,
                    (Define.PlayerClass)_characterSelectorNgo.ChoiceCharacter);

                _sceneManagerEx.ResetBossSceneProgress();
                _sceneManagerEx.OnClientLoadedEvent += ClientLoadedEvent;
                
                _sceneManagerEx.NetworkLoadScene(Define.SceneName.GamePlayScene);
            }
            catch (Exception e)
            {
                _relayManager.NgoRPCCaller.SetGameStartLoadingRpc(false);
                UtilDebug.LogError($"Failed to load gameplay scene. {e}");
                _isHostButtonTrigger = false;
            }

            void ClientLoadedEvent(ulong clientId)
            {
                _relayManager.NgoRPCCaller.GetPlayerChoiceCharacterRpc(clientId);
                UtilDebug.Log(_sceneManagerEx.GetCurrentScene.CurrentSceneName + "씬네임" + "플레이어 ID" + clientId);
            }
        }

        public void ButtonState(bool state)
        {
            _readyButtonState = state;
            ButtonImageChanged(state == false ? ReadyButtonStateEnum.TrueState : ReadyButtonStateEnum.CancelState);
        }

        private void ButtonImageChanged(ReadyButtonStateEnum state)
        {
            ReadyButtonImages buttonimages = _readyButtonStateValue[(int)state];

            _buttonReady.image.sprite = buttonimages.ReadyButtonImage;
            _buttonText.text = buttonimages.ReadyButtonText;
            _buttonText.color = buttonimages.ReadyButtonTextColor;
        }
    }
}
