using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameManagers.LoginManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.SceneManagement;
using GameManagers.SocketManagement;
using GameManagers.UIManagement;
using GameManagers.VivoxManagement;

using ScenesScripts;
using ScenesScripts.CommonInstaller.Interfaces;
using UI.Popup.PopupUI;
using UI.Scene.SceneUI;
using UI.SubItem;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace GameManagers.LobbyManagement
{

    public interface IDisconnectStrategy
    { 
        UniTask HandleDisconnectAsync(ulong disconnectID,RelayManager relayManager,LobbyManager lobbyManager, SceneManagerEx sceneManger);
    }
    
    public class LobbyManager : IInitializable,IRegistrar<IDisconnectStrategy>
    {
        private readonly IResourcesServices _resourcesServices;
        private readonly IPlayerLogininfo _playerLogininfo;
        private readonly IPlayerIngameLogininfo _playerIngameLogininfo; //얘는 지금 Null인 상태 여기서 지정해줘야함.
        private readonly IUIManagerServices _uiManager;
        private readonly ISendMessage _sendMessage;
        private readonly IVivoxSession _vivoxSession;
        private readonly SceneManagerEx _sceneManagerEx;
        private readonly RelayManager _relayManager;
        private readonly SocketEventManager _socketEventManager;
        private readonly SignalBus _signalBus;
        private IDisconnectStrategy _currentDisconnectStrategy;
        private readonly IDisconnectStrategy _defaultDisconnectStrategy;//기본 끊어지는 값
        private ILobbyEvents _lobbyEvents; //현재 로비의 이벤트

        [Inject]
        public LobbyManager(
            IResourcesServices resourcesServices,
            IPlayerLogininfo playerLogininfo,
            IPlayerIngameLogininfo playerIngameLogininfo,
            IUIManagerServices uiManager,
            ISendMessage sendMessage,
            IVivoxSession vivoxSession,
            SceneManagerEx sceneManagerEx,
            RelayManager relayManager,
            SocketEventManager socketEventManager, 
            SignalBus signalBus,
            IDisconnectStrategy defaultDisconnectStrategy)
        {
            _resourcesServices = resourcesServices;
            _playerLogininfo = playerLogininfo;
            _playerIngameLogininfo = playerIngameLogininfo;
            _uiManager = uiManager;
            _sendMessage = sendMessage;
            _vivoxSession = vivoxSession;
            _sceneManagerEx = sceneManagerEx;
            _relayManager = relayManager;
            _socketEventManager = socketEventManager;
            _signalBus = signalBus;
            _defaultDisconnectStrategy = defaultDisconnectStrategy;

            _currentDisconnectStrategy = _defaultDisconnectStrategy;//초기값 저장
        }


        enum LoadingProcess
        {
            VivoxInitalize,
            UnityServices,
            SignInAnonymously,
            CheckAlreadyLogInID,
            TryJoinLobby,
            VivoxLogin
        }

        private const string WaitLobbyName = "WaitLobby";
        private const int RoomPageSize = 4;
        private const int AlreadyConnectedRetryCount = 5;
        private const int AlreadyConnectedRetryDelayMs = 1000;
        private const int RelayCodeRetryCount = 3;
        private const int RelayCodeRetryDelayMs = 500;

        private bool _isDoneLobbyInitEvent = false;
        private Lobby _currentLobby;
        private bool _isRefreshing = false;
        private readonly List<string> _roomPageTokens = new List<string>() { null };
        private int _currentRoomPageIndex = 0;
        private string _nextRoomPageToken;
        private bool[] _taskChecker;
        private Action<bool> _lobbyLoading;
        private Action _initDoneEvent;
        private Action _hostChangeEvent;
        private CancellationTokenSource _heartbeatCts;
        public bool IsHostMigrationPending { get; private set; }
        private PlayerIngameLoginInfo CurrentPlayerIngameInfo
        {
            get => _playerIngameLogininfo.GetPlayerIngameLoginInfo();
            set => _playerIngameLogininfo.SetPlayerIngameLoginInfo(value);
        }

        private string PlayerID => CurrentPlayerIngameInfo.Id;
        public int CurrentRoomPage => _currentRoomPageIndex + 1;
        public bool CanMovePreviousRoomPage => _currentRoomPageIndex > 0;
        public bool CanMoveNextRoomPage => string.IsNullOrEmpty(_nextRoomPageToken) == false;


        public event Action<bool> LobbyLoadingEvent
        {
            add { UniqueEventRegister.AddSingleEvent(ref _lobbyLoading, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _lobbyLoading, value); }
        }

        public event Action InitDoneEvent
        {
            add { UniqueEventRegister.AddSingleEvent(ref _initDoneEvent, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _initDoneEvent, value); }
        }

        public event Action HostChangeEvent
        {
            add { UniqueEventRegister.AddSingleEvent(ref _hostChangeEvent, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _hostChangeEvent, value); }
        }

        public void Initialize()
        {
            _signalBus.Subscribe<RelayDisconnectedSignal>(signal => 
                _currentDisconnectStrategy.HandleDisconnectAsync(signal.DisconnectedId,_relayManager,this,_sceneManagerEx).Forget());
        }
        

        public void Register(IDisconnectStrategy idisconnteStrategy)
        {
            _currentDisconnectStrategy = idisconnteStrategy;
        }
        public void Unregister(IDisconnectStrategy strategy)
        {
            if (ReferenceEquals(_currentDisconnectStrategy, strategy))
            { //항상 기본값은 _defaultDisconnectStrategy로 두고
                //각 씬마다 행동 하고 싶은 IDisconnectStrategy를 새로 바인드해 그 씬에서만 쓸 수 있는
                //연결끊기 로직을 수행
                _currentDisconnectStrategy = _defaultDisconnectStrategy;
            }
        }
        
        public bool IsDoneLobbyInitEvent
        {
            get => _isDoneLobbyInitEvent;
        }

        public void TriggerLobbyLoadingEvent(bool lobbyState)
        {
            _lobbyLoading?.Invoke(lobbyState);
        }

        public async UniTask<Lobby> GetCurrentLobby()
        {
            if (_currentLobby == null)
                return null;

            _currentLobby = await GetLobbyAsyncCustom(_currentLobby.Id);
            return _currentLobby;
        }

        public async UniTask<bool> InitLobbyScene(bool showAlreadyConnectedDialog = false)
        {
            bool isalready = false;
            int retryLimit = showAlreadyConnectedDialog ? AlreadyConnectedRetryCount : 0;
            SetLoadingTask(Enum.GetValues(typeof(LoadingProcess)).Length);
            SubscribeSceneEvent();
            try
            {
                for (int retryCount = 0; retryCount <= retryLimit; retryCount++)
                {
                    await JoinAuthenticationService();
                    // Unity Services 초기화
                    isalready = await IsAlreadyLogInNickNameinLobby(CurrentPlayerIngameInfo.PlayerNickName);

                    if (isalready == false)
                        break;

                    if (retryCount >= retryLimit)
                        break;

                    UtilDebug.Log($"Already connected. Retrying lobby init. TryCount: {retryCount + 1}");
                    await UniTask.Delay(AlreadyConnectedRetryDelayMs, ignoreTimeScale: true);
                }

                _taskChecker[(int)LoadingProcess.CheckAlreadyLogInID] = true;
                if (isalready is true)
                {
                    UtilDebug.Log("이미 접속되어있음");
                    if (showAlreadyConnectedDialog)
                    {
                        ShowAlreadyConnectedDialog();
                    }

                    return true;
                }

                await TryJoinLobbyByNameOrCreateWaitLobby();
                _taskChecker[(int)LoadingProcess.TryJoinLobby] = true;
                return false;
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"Initialization failed: {ex.Message}");
                if (_sceneManagerEx.GetCurrentScene is LoadingScene loadingScene)
                {
                    loadingScene.IsErrorOccurred = true;
                }

                throw;
            }

            void SetLoadingTask(int taskLength)
            {
                _taskChecker = new bool[taskLength];
                _sceneManagerEx.SetCheckTaskChecker(_taskChecker);
            }

            void SubscribeSceneEvent()
            {
                _relayManager.SceneLoadInitializeRelayServer();
                InitializeVivoxEvent();
                InitializeLobbyEvent();
                _taskChecker[(int)LoadingProcess.VivoxInitalize] = true;
            }

            async UniTask JoinAuthenticationService()
            {
                await UnityServices.InitializeAsync();
                _taskChecker[(int)LoadingProcess.UnityServices] = true;
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    await LogoutAndAllLeaveLobby();
                    UtilDebug.Log("Logging out from previous session...");
                    if (_vivoxSession != null)
                    {
                        await _vivoxSession.LogoutOfVivoxAsync();
                    }
                    AuthenticationService.Instance.SignOut();
                }

                await SignInAnonymouslyAsync();
                _taskChecker[(int)LoadingProcess.SignInAnonymously] = true;
            }

            async UniTask<bool> IsAlreadyLogInNickNameinLobby(string usernickName)
            {
                try
                {
                    QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();
                    string currentPlayerId = PlayerID;

                    UtilDebug.Log(
                        $"[LobbyDuplicateCheck] Start. nickname='{usernickName}', currentPlayerId='{currentPlayerId}', lobbyCount={response.Results.Count}");

                    if (response.Results.Count <= 0)
                        return false;

                    foreach (Lobby lobby in response.Results)
                    {
                        UtilDebug.Log(
                            $"[LobbyDuplicateCheck] Inspect lobby. lobbyId='{lobby.Id}', lobbyName='{lobby.Name}', hostId='{lobby.HostId}', playerCount={lobby.Players.Count}");

                        foreach (Player player in lobby.Players)
                        {
                            if (player.Data == null && PlayerID == player.Id) //나인데 데이터를 할당 못받았으면,다시 초기화
                            {
                                UtilDebug.LogError($" Player {player.Id} in lobby {lobby.Id} has NULL Data!");
                                return await Utill.RateLimited(async () => await InitLobbyScene(), 5000); // 재시도
                            }

                            foreach (KeyValuePair<string, PlayerDataObject> data in player.Data)
                            {
                                if (player.Data == null)
                                    continue;

                                if (PlayerID == player.Id) //자기자신은 건너뛰기
                                    continue;

                                if (data.Key != "NickName")
                                {
                                    continue;
                                }
                                else
                                {
                                    if (data.Value.Value != usernickName)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        UtilDebug.Log(
                                            $"[LobbyDuplicateCheck] Duplicate nickname found. currentPlayerId='{currentPlayerId}', matchedPlayerId='{player.Id}', lobbyId='{lobby.Id}', lobbyName='{lobby.Name}', nickname='{data.Value.Value}'");

                                        await LogoutAndAllLeaveLobby();
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }
                catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    return await Utill.RateLimited(() => IsAlreadyLogInNickNameinLobby(usernickName)); // 재시도
                }

                catch (Exception notSetObjectException) when (notSetObjectException.Message.Equals(
                                                                  "Object reference not set to an instance of an object"))
                {
                    UtilDebug.Log("The Lobby hasnt reference so We Rate Secend");
                    return await Utill.RateLimited(() => IsAlreadyLogInNickNameinLobby(usernickName));
                }
                catch (Exception ex)
                {
                    UtilDebug.LogError($"Failed to query lobbies: {ex.Message}");
                    return false;
                }
            }
        }

        private void ShowAlreadyConnectedDialog()
        {
            if (_uiManager.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == true)
            {
                dialog.GetComponent<Canvas>().sortingOrder = 100;
                dialog.AfterAlertEvent(() => _sceneManagerEx.LoadScene(Define.SceneName.LoginScene))
                    .AlertSetText("접속 오류", "이미 ID가 로비에 접속중입니다. \n 잠시후 다시 접속해주세요.");
            }
        }


        private void SetVivoxTaskCheker()
        {
            _taskChecker[(int)LoadingProcess.VivoxLogin] = true;
            UtilDebug.Log("VivoxLogin켜짐");
        }

        private async UniTaskVoid HeartbeatLoop(string lobbyId, float waitTimeSeconds, CancellationToken token)
        {
            TimeSpan delay = TimeSpan.FromSeconds(waitTimeSeconds);

            try
            {
                while (token.IsCancellationRequested == false) //토큰이 취소 요청을 보내기 전까지 반복
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                    UtilDebug.Log("HeartBeat Sent (Ping)");

                    await UniTask.Delay(delay, ignoreTimeScale: true, cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
                UtilDebug.Log("Heartbeat Loop Stopped.");
            }
            catch (Exception ex)
            {
                UtilDebug.LogWarning($"Heartbeat Error: {ex.Message}");
                StopHeartbeat(); // 에러 발생 시 안전하게 종료
            }
        }

        private void CheckHostAndSendHeartBeat(Lobby lobby, float interval = 15f)
        {
            try
            {
                UtilDebug.Log($"로비의 호스트 ID:{lobby.HostId} 나의 아이디{PlayerID}");
                StopHeartbeat();

                if (lobby.HostId == PlayerID)
                {
                    UtilDebug.Log("하트비트 시작 (UniTask)");
                    _heartbeatCts = new CancellationTokenSource();
                    HeartbeatLoop(lobby.Id, interval, _heartbeatCts.Token).Forget();
                }
            }
            catch (Exception e)
            {
                UtilDebug.Log(e);
            }
        }

        private async UniTask JoinRelayServer(Lobby lobby, Func<Lobby, UniTask> checkHostAndGuestEvent)
        {
            if (checkHostAndGuestEvent != null)
            {
                await checkHostAndGuestEvent.Invoke(lobby);
            }
        }

        private async UniTask RegisteLobbyCallBack(
            Lobby lobby,
            Func<ILobbyChanges, UniTask> onLobbyChangeEvent,
            Func<List<LobbyPlayerJoined>, UniTask> onPlayerJoinedEvent = null,
            Func<List<int>, UniTask> onPlayerLeftEvent = null)
        {
            if (_lobbyEvents != null) //로비에 등록된 이벤트가 있다면 해제
            {
                await _lobbyEvents.UnsubscribeAsync();
                _lobbyEvents = null;
            }
            
            string subscribedLobbyId = lobby.Id;
            LobbyEventCallbacks lobbycallbacks = new LobbyEventCallbacks();
            
            lobbycallbacks.LobbyChanged += (lobbyChanges) =>
            {
                if (_currentLobby == null || _currentLobby.Id != subscribedLobbyId)
                {
                    return;
                }
                
                onLobbyChangeEvent?.Invoke(lobbyChanges).Forget();
            };
            
            lobbycallbacks.PlayerJoined += (lobbyPlayerJoined) =>
            {
                if (_currentLobby == null || _currentLobby.Id != subscribedLobbyId)
                {
                    return;
                }
                onPlayerJoinedEvent?.Invoke(lobbyPlayerJoined).Forget();
            };
            lobbycallbacks.PlayerLeft += (playerLeftList) =>
            {
                if (_currentLobby == null || _currentLobby.Id != subscribedLobbyId)
                {
                    return;
                }
                onPlayerLeftEvent?.Invoke(playerLeftList).Forget();
            };
            try
            {
                _lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, lobbycallbacks);
            }
            catch (LobbyServiceException ex)
            {
                switch (ex.Reason)
                {
                    case LobbyExceptionReason.AlreadySubscribedToLobby:
                        UtilDebug.LogWarning(
                            $"Already subscribed to lobby[{lobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}");
                        break;
                    case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
                        UtilDebug.LogError(
                            $"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}");
                        throw;
                    case LobbyExceptionReason.LobbyEventServiceConnectionError:
                        UtilDebug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}");
                        throw;
                    default: throw;
                }
            }
        }

        private async UniTask CheckHostRelay(Lobby lobby)
        {
            if (lobby.HostId != PlayerID)
                return;

            try
            {
                string joincode = await _relayManager.StartHostWithRelay(lobby.MaxPlayers);
                UtilDebug.Log(lobby.Name + "로비의 이름");
                await InjectionRelayJoinCodeintoLobby(lobby, joincode);
            }
            catch (LobbyServiceException timeLimmitException) when (timeLimmitException.Message.Contains(
                                                                        "Rate limit has been exceeded"))
            {
                await Utill.RateLimited(async () => await CheckHostRelay(lobby));
                return;
            }
        }

        private async UniTask CheckClientRelay(Lobby lobby)
        {
            await TryJoinClientRelayWithRetry(lobby);
        }

        private async UniTask<bool> TryJoinClientRelayWithRetry(Lobby lobby)
        {
            if (lobby == null)
            {
                UtilDebug.Log("현재 방이 없습니다.");
                return false;
            }


            if (lobby.HostId == PlayerID)
                return true;

            string lobbyId = lobby.Id;
            for (int retryCount = 0; retryCount <= RelayCodeRetryCount; retryCount++)
            {
                if (TryGetRelayCode(lobby, out string joincode))
                {
                    UtilDebug.Log($"찾으려는 릴레이코드{joincode}");
                    return await _relayManager.JoinGuestRelay(joincode);
                }

                if (retryCount >= RelayCodeRetryCount)
                    break;

                await UniTask.Delay(RelayCodeRetryDelayMs, ignoreTimeScale: true);

                try
                {
                    lobby = await GetLobbyAsyncCustom(lobbyId);
                }
                catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyNotFound)
                {
                    return false;
                }
            }

            return false;
        }

        private bool TryGetRelayCode(Lobby lobby, out string joincode)
        {
            joincode = null;

            if (lobby?.Data == null)
                return false;

            if (lobby.Data.TryGetValue("RelayCode", out DataObject relayCodeData) == false)
                return false;

            if (relayCodeData == null || string.IsNullOrEmpty(relayCodeData.Value))
                return false;

            joincode = relayCodeData.Value;
            return true;
        }

        private bool IsJoinableRoomLobby(Lobby lobby)
        {
            if (lobby == null)
                return false;

            if (TryGetRelayCode(lobby, out _) == false)
                return false;

            if (lobby.Data.TryGetValue("RoomState", out DataObject roomStateData) == false ||
                roomStateData == null ||
                roomStateData.Value != "Open")
                return false;

            return lobby.Players != null && lobby.Players.Any(player => player.Id == lobby.HostId);
        }

        private void ShowJoinRelayFailedDialog()
        {
            if (_uiManager.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == true)
            {
                dialog.AlertSetText("오류", "방 접속 정보가 없습니다.\n방 목록을 새로고침 후 다시 시도해주세요.")
                    .AfterAlertEvent(async () =>
                    {
                        await ReFreshRoomList();
                    });
            }
        }

        private void StopHeartbeat()
        {
            if (_heartbeatCts == null) return;

            _heartbeatCts.Cancel(); //캔슬 요청
            _heartbeatCts.Dispose(); //메모리 해제
            _heartbeatCts = null;
        }

        private async UniTask TryJoinLobbyByNameOrCreateLobby(string lobbyName, int maxPlayers,
            CreateLobbyOptions lobbyOption)
        {
            try
            {
                Lobby lobbyResult = await AvailableLobby(lobbyName);
                if (lobbyResult == null)
                {
                    UtilDebug.Log("There is not WaitLobby, so Create Wait Lobby");
                    _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOption);
                }
                else
                {
                    UtilDebug.Log("Find WaitLobby, Join to Lobby");
                    _currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyResult.Id);
                }

                Lobby currentLobby = await GetCurrentLobby();
                
                CheckHostAndSendHeartBeat(currentLobby);
                Func<ILobbyChanges, UniTask> waitLobbyDataChangedEvent = null;
                waitLobbyDataChangedEvent += async (changes) => await OnLobbyHostChangeEvent(changes);
                waitLobbyDataChangedEvent += async (changes) => await NotifyPlayerCreateRoomEvent(changes);
                
                await JoinLobbyInitalize(currentLobby, waitLobbyDataChangedEvent);
                await _sendMessage.SendSystemMessageAsync(
                    $"{CurrentPlayerIngameInfo.PlayerNickName}님이 접속하셨습니다");
            }
            catch (LobbyServiceException alreayException) when (alreayException.Message.Contains(
                                                                    "player is already a member of the lobby"))
            {
                UtilDebug.Log("플레이어가 이미 접속중입니다. 정보삭제후 재진입을 시도 합니다.");
                _sceneManagerEx.SetCheckTaskChecker(_taskChecker);
                await InitLobbyScene();
            }
            catch (LobbyServiceException TimeLimmitException) when (TimeLimmitException.Message.Contains(
                                                                        "Rate limit has been exceeded"))
            {
                await Utill.RateLimited(() => TryJoinLobbyByNameOrCreateLobby(lobbyName, maxPlayers, lobbyOption));
            }

            catch (KeyNotFoundException keynotFoundExceoption) when (keynotFoundExceoption.Message.Contains(
                                                                         "The given key 'RelayCode' was not present in the dictionary"))
            {
                UtilDebug.Log("릴레이코드가 없습니다. 다시 찾습니다");
                await Utill.RateLimited(() => TryJoinLobbyByNameOrCreateLobby(lobbyName, maxPlayers, lobbyOption));
            }
            catch (Exception ex)
            {
                UtilDebug.Log(ex);
                throw;
            }

            async UniTask OnLobbyHostChangeEvent(ILobbyChanges lobbyChanges)
            {
                try
                {
                    // 2026-05-26: WaitLobby 호스트가 방 생성으로 빠질 때 HostId 값이 없는 PlayerLeft 이벤트로 올 수 있어 최신 로비 기준으로 재판단한다.
                    if (lobbyChanges.HostId.Changed == false && lobbyChanges.PlayerLeft.Changed == false)
                        return;

                    Lobby currentLobby = await GetCurrentLobby();
                    if (currentLobby == null)
                        return;

                    CheckHostAndSendHeartBeat(currentLobby);
                }
                catch (Exception errer)
                {
                    UtilDebug.Log(errer);
                }
            }

            // 모든 LobbyChanged 이벤트에서 호출
            async UniTask NotifyPlayerCreateRoomEvent(ILobbyChanges lobbyChanges)
            {
                try
                {
                    // PlayerData 영역에 변경이 없다면 종료
                    if (lobbyChanges.PlayerData.Value == null)
                        return;

                    //PlayerData.Value :  Dictionary<int /*player-index*/, LobbyPlayerChanges>
                    foreach (KeyValuePair<int, LobbyPlayerChanges> changedData in lobbyChanges.PlayerData.Value)
                    {
                        LobbyPlayerChanges playerChanges = changedData.Value;

                        //그 플레이어의 Data 중 실제로 값이 바뀐(Changed) 키 목록
                        var dataChanges = playerChanges.ChangedData;

                        if (!dataChanges.Changed || dataChanges.Value == null)
                            continue; // 데이터 변경이 없으면 패스

                        //LastCreatedRoom 필드가 바뀌었는지 확인
                        if (dataChanges.Value.TryGetValue("LastCreatedRoom", out var roomFieldChange) &&
                            roomFieldChange.Changed) // ← key 값이 변경된 경우에만
                        {
                            // roomFieldChange.Value : PlayerDataObject
                            string newRoomId = roomFieldChange.Value.Value;
                            UtilDebug.Log($"다른 플레이어가 새 방을 만들었습니다!  RoomId = {newRoomId}");

                            // 필요하다면 즉시 방 목록 갱신
                            await ReFreshRoomList();
                        }
                    }
                }
                catch (Exception error)
                {
                    UtilDebug.Log(error);
                }
            }
        }

        private async UniTask OnRoomLobbyChangeHostEventAsync(ILobbyChanges lobbyChanges)
        {
            if (lobbyChanges.HostId.Value == PlayerID)
            {
                _lobbyLoading?.Invoke(true);
                try
                {
                    Lobby currentLobby = await GetCurrentLobbyForRoomLobbyChangeAsync();
                    if (currentLobby == null)
                        return;

                    await CheckHostRelay(currentLobby);
                    CheckHostAndSendHeartBeat(currentLobby);
                    _hostChangeEvent?.Invoke();
                }
                finally
                {
                    _lobbyLoading?.Invoke(false);
                }
            }

            if (lobbyChanges.Data.Value != null &&
                lobbyChanges.Data.Value.TryGetValue("RelayCode", out var relayData) && relayData.Changed)
            {
                _lobbyLoading?.Invoke(true);
                try
                {
                    var newCode = relayData.Value;
                    Lobby currentLobby = await GetCurrentLobbyForRoomLobbyChangeAsync();
                    if (currentLobby == null)
                        return;

                    await CheckClientRelay(currentLobby);
                }
                finally
                {
                    _lobbyLoading?.Invoke(false);
                }
            }
        }

        private async UniTask<Lobby> GetCurrentLobbyForRoomLobbyChangeAsync()
        {
            try
            {
                return await GetCurrentLobby();
            }
            catch (LobbyServiceException exception) when (exception.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                UtilDebug.Log($"이미 사라진 RoomLobby 변경 콜백을 무시합니다. {exception.Message}");
                return null;
            }
            catch (NullReferenceException exception)
            {
                UtilDebug.Log($"이미 사라진 RoomLobby 변경 콜백을 무시합니다. {exception.Message}");
                return null;
            }
        }

        private async UniTask InjectionRelayJoinCodeintoLobby(Lobby lobby, string joincode)
        {
            if (joincode == null || lobby == null)
            {
                UtilDebug.Log(
                    $"Data has been NULL, is Check Lobby Null?: {lobby.Equals(null)} is Check JoinCode Null? {lobby.Equals(null)}");
                return;
            }

            _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, joincode) },
                    {
                        "RoomState",
                        new DataObject(
                            DataObject.VisibilityOptions.Public,
                            "Open",
                            index: DataObject.IndexOptions.S2)
                    }
                }
            });
        }

        private async UniTask JoinLobbyInitalize(Lobby lobby,
            Func<ILobbyChanges, UniTask> OnLobbyChangeEvent,
            Func<List<LobbyPlayerJoined>, UniTask> OnPlayerJoinedEvent = null,
            Func<List<int>, UniTask> OnPlayerLeftEvent = null)
        {
            _isDoneLobbyInitEvent = false;
            try
            {
                await InputPlayerDataIntoLobby(lobby); //로비에 있는 player에 닉네임추가
                await _vivoxSession.JoinChannelAsync(lobby.Id); //비복스 연결
                await RegisteLobbyCallBack(lobby, OnLobbyChangeEvent, OnPlayerJoinedEvent, OnPlayerLeftEvent);
                _initDoneEvent?.Invoke(); //호출이 완료되었을때 이벤트 콜백
                _isDoneLobbyInitEvent = true;
            }

            catch (Exception ex)
            {
                UtilDebug.LogError($"JoinRoomInitalize 중 오류 발생: {ex}");
                _isDoneLobbyInitEvent = false;
                throw; // 상위 호출부에 예외를 전달
            }
        }

        private async UniTask<Lobby> GetLobbyAsyncCustom(string lobbyId)
        {
            try
            {
                return await LobbyService.Instance.GetLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.RateLimited)
            {
                return await Utill.RateLimited(() => GetLobbyAsyncCustom(lobbyId));
            }
        }

        private async UniTask<QueryResponse> GetQueryLobbiesAsyncCustom(QueryLobbiesOptions queryFilter = null)
        {
            try
            {
                return await LobbyService.Instance.QueryLobbiesAsync(queryFilter);
            }
            catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.RateLimited)
            {
                return await Utill.RateLimited(() => GetQueryLobbiesAsyncCustom(queryFilter));
            }
            catch (NullReferenceException e) //5.6일 수정 이전(5.3)에 로비 탐색에 대한 로직을 수정하니 쿼리가 Null이 나오는 에러가 떠서 예외처리
            {
                UtilDebug.LogWarning($"Lobby query failed with NullReferenceException. Retrying. {e}");
                return await Utill.RateLimited(() => GetQueryLobbiesAsyncCustom(queryFilter));
            }
            catch (Exception ex)
            {
                UtilDebug.Log(ex);
                throw;
            }
        }

        private async UniTask LeaveAllLobby()
        {
            _lobbyLoading?.Invoke(true);
            List<Lobby> lobbyinPlayerList = await GetAllLobbyinPlayerList();
            UtilDebug.Log(
                $"[LobbyLeaveAll] Start. playerId='{PlayerID}', lobbyCount={lobbyinPlayerList.Count}");

            foreach (Lobby lobby in lobbyinPlayerList)
            {
                UtilDebug.Log(
                    $"[LobbyLeaveAll] Leaving lobby. playerId='{PlayerID}', lobbyId='{lobby.Id}', lobbyName='{lobby.Name}', hostId='{lobby.HostId}', playerCount={lobby.Players.Count}");

                await ExitLobbyAsync(lobby);
                UtilDebug.Log($"{lobby}에서 나갔습니다.");
                StopHeartbeat();
            }

            _lobbyLoading?.Invoke(false);
        }

        private async UniTask<List<Lobby>> GetAllLobbyinPlayerList()
        {
            //필터 옵션에서 모든 플레이어를 검사하는 필터 옵션은 없으므로 따로 만듦
            List<Lobby> lobbyinPlayerList = new List<Lobby>();
            QueryResponse allLobbyResponse = await GetQueryLobbiesAsyncCustom();
            foreach (Lobby lobby in allLobbyResponse.Results)
            {
                if (lobby.Players.Any(player => player.Id == PlayerID))
                {
                    lobbyinPlayerList.Add(lobby);
                }
            }

            return lobbyinPlayerList;
        }

        private async UniTask<string> SignInAnonymouslyAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                UtilDebug.Log("Sign in anonymously succeeded!");

                string anonymouslyID = AuthenticationService.Instance.PlayerId;
                UtilDebug.Log($"플레이어 ID 만들어짐{anonymouslyID}");

                CurrentPlayerIngameInfo = new PlayerIngameLoginInfo(_playerLogininfo.PlayerNickName, anonymouslyID);

                // Shows how to get the playerID
                return anonymouslyID;
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                UtilDebug.LogError(ex);
                return null;
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                UtilDebug.LogError(ex);
                return null;
            }
        }

        private async UniTask InputPlayerDataIntoLobby(Lobby lobby)
        {
            if (lobby == null)
                return;

            Dictionary<string, PlayerDataObject> updatedData = new Dictionary<string, PlayerDataObject>
            {
                {
                    "NickName",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,
                        $"{CurrentPlayerIngameInfo.PlayerNickName}")
                },
            };
            try
            {
                await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, PlayerID, new UpdatePlayerOptions
                {
                    Data = updatedData
                });

                UtilDebug.Log($"로비ID: {lobby.Id} \t 플레이어ID: {PlayerID} 정보가 입력되었습니다.");
            }
            catch (Exception error)
            {
                UtilDebug.LogError($"에러 발생{error}");
            }
        }

        private async UniTask RemovePlayerData(Lobby lobby)
        {
            UtilDebug.Log($"로비ID{lobby.Id} \t 플레이어ID{PlayerID} 정보가 제거되었습니다.");
            await LobbyService.Instance.RemovePlayerAsync(lobby.Id, PlayerID);
        }


        public async UniTask TryJoinLobbyByNameOrCreateWaitLobby()
        {
            if (_currentLobby != null)
                await ExitLobbyAsync(_currentLobby); //이쪽은 문제 없음

            try
            {
                CreateLobbyOptions waitLobbyoption = new CreateLobbyOptions()
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
                    {
                        { WaitLobbyName, new DataObject(DataObject.VisibilityOptions.Public, "PlayerWaitLobbyRoom") }
                    }
                };
                await TryJoinLobbyByNameOrCreateLobby(WaitLobbyName, 100, waitLobbyoption);
            }
            catch (LobbyServiceException playerNotFound) when (playerNotFound.Message.Contains("player not found"))
            {
                UtilDebug.Log("Player Not Found");
                await InitLobbyScene();
                return;
            }
            catch (Exception e)
            {
                UtilDebug.Log("여기서 문제가 발생" + e);
            }
        }

        public async UniTask<Lobby> AvailableLobby(string lobbyname)
        {
            List<Lobby> fillteredLobbyList = null;
            QueryLobbiesOptions lobbyNameFillter = new QueryLobbiesOptions()
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.Name,
                        op: QueryFilter.OpOptions.EQ, // EQ는 "같은 이름"일 때만
                        value: lobbyname) // 이 이름과 정확히 일치하는 로비만 검색
                }
            };
            QueryResponse queryResponse = await GetQueryLobbiesAsyncCustom(lobbyNameFillter);

            if (queryResponse is null) //이름으로 못찾았다.
            {
                return null;
            }

            fillteredLobbyList = queryResponse.Results;


            foreach (Lobby lobby in fillteredLobbyList)
            {
                if (lobby.Players.Count >= 1)
                {
                    return lobby;
                }
            }

            return null;
        }

        public async UniTask RemoveLobbyAsync(Lobby lobby)
        {
            if (ReferenceEquals(_currentLobby, lobby))
                _currentLobby = null;

            if (lobby.HostId != CurrentPlayerIngameInfo.Id)
                return;

            StopHeartbeat(); //하트비트 제거
            Lobby currentLobby = await GetLobbyAsyncCustom(lobby.Id); //현재의 로비를 가져와야한다.
            await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
        }

        // 게임 시작 직전에 현재 캐릭터 선택 로비를 참가 불가 상태로 바꾸고 삭제한다.
        // 이 메서드가 true를 반환한 뒤에만 GamePlayScene 네트워크 씬 전환을 진행한다.
        public async UniTask<bool> CloseCurrentRoomLobbyForGameStartAsync()
        {
            try
            {
                Lobby currentLobby = await GetCurrentLobby();

                if (currentLobby == null)
                    return false;

                if (currentLobby.HostId != CurrentPlayerIngameInfo.Id)
                    return false;

                // 삭제 요청 전에 먼저 Open 목록 필터에서 빠지도록 RoomState를 Closing으로 바꾼다.
                await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {
                            "RoomState",
                            new DataObject(
                                DataObject.VisibilityOptions.Public,
                                "Closing",
                                index: DataObject.IndexOptions.S2)
                        }
                    }
                });

                // 로비 삭제 응답을 받은 뒤에만 현재 로비 참조를 비우고 성공 처리한다.
                StopHeartbeat();
                await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
                _currentLobby = null;
                return true;
            }
            catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.RateLimited)
            {
                return await Utill.RateLimited(CloseCurrentRoomLobbyForGameStartAsync);
            }
            catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                // 이미 서비스에서 로비가 사라진 상태라면 게임 시작을 막을 이유가 없으므로 성공 처리한다.
                StopHeartbeat();
                _currentLobby = null;
                return true;
            }
        }

        public async UniTask ExitLobbyAsync(Lobby lobby)
        {
            if (lobby == null)
                return;

            // 2026-05-26: WaitLobby 정리 중 방 로비 하트비트가 끊기지 않도록, 현재 로비를 나갈 때만 하트비트를 정지한다.
            //이전에는 로비의 ID구분없이 현재 하트비트를끊으니 방을 팔때 하트 비트가 안울리는 경우가 생겨 수정함
            bool isLeavingCurrentLobby = _currentLobby != null && _currentLobby.Id == lobby.Id;

            try
            {
                if (isLeavingCurrentLobby)
                    StopHeartbeat(); //하트비트 제거

                Lobby currentLobby;
                try
                {
                    currentLobby = await GetLobbyAsyncCustom(lobby.Id); //현재의 로비를 가져와야한다.
                }
                catch (NullReferenceException)
                {
                    // 2026-05-26: Unity Lobby SDK의 GetLobbyAsync 내부 오류가 나가기를 막으면 WaitLobby 호스트 이전이 진행되지 않아 기존 로비 정보로 계속 처리한다.
                    currentLobby = lobby;
                }

                bool isHost = currentLobby.HostId == PlayerID;
                bool isAlone = currentLobby.Players.Count <= 1;

                if (isHost && isAlone)
                {
                    //내가 호스트도 로비에 나만 남았다면 로비삭제
                    await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
                    UtilDebug.Log("로비삭제");
                }
                else
                {
                    //마지막에 남은 사람이 나말고 다른 사람도 있는데, 내가 호스트인경우
                    UtilDebug.Log("로비데이터 내 내 데이터 삭제");
                    
                    if (isHost)
                    {
                        await DeleteRelayCodefromLobbyAsync(currentLobby);
                    }
                    await RemovePlayerData(lobby);
                }

                if (isLeavingCurrentLobby)
                    _currentLobby = null;
            }
            catch (System.ObjectDisposedException disposedException)
            {
                UtilDebug.Log($"이미 객체가 제거되었습니다.{disposedException.Message}");
            }
            catch (Exception e)
            {
                UtilDebug.Log($"LeaveLobby 메서드 안에서의 에러{e}");
            }
        }

        public async UniTask CreateLobby(string lobbyName, int maxPlayers, CreateLobbyOptions options)
        {
            try
            {
                Lobby waitLobby = await GetCurrentLobby();

                _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                if (_currentLobby != null && waitLobby != null)
                {
                    UtilDebug.Log($"로비 만들어짐{_currentLobby.Name}");
                }

                CheckHostAndSendHeartBeat(_currentLobby);
                await JoinLobbyInitalize(_currentLobby, OnRoomLobbyChangeHostEventAsync);
                await JoinRelayServer(_currentLobby, CheckHostRelay);

                if (_currentLobby != null && waitLobby != null)
                {
                    await CreateRoomWriteinWaitLobby(waitLobby.Id, PlayerID);
                    await ExitLobbyAsync(waitLobby);
                }
            }

            catch (Exception e)
            {
                UtilDebug.Log($"An error occurred while creating the room.{e}");
                throw;
            }

            async UniTask CreateRoomWriteinWaitLobby(string lobbyId, string playerId)
            {
                await LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, new UpdatePlayerOptions()
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        {
                            "LastCreatedRoom",
                            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, lobbyName)
                        }
                    }
                });
            }
        }

        public async UniTask<Lobby> JoinLobbyByID(string lobbyID, string password = null)
        {
            Lobby preLobby = _currentLobby;
            Lobby nextLobby;
            try
            {
                Lobby latestLobby = await GetLobbyAsyncCustom(lobbyID);
                if (IsJoinableRoomLobby(latestLobby) == false)
                {
                    ShowJoinRelayFailedDialog();
                    return null;
                }

                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions() { Password = password };
                nextLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID, options);
                
                UtilDebug.Log(nextLobby.Id + "다음 로비 아이디");
            }
            catch (LobbyServiceException wrongPw) when (wrongPw.Reason == LobbyExceptionReason.IncorrectPassword)
            {
                throw;
            }
            catch (LobbyServiceException timeLimit) when (timeLimit.Reason == LobbyExceptionReason.RateLimited)
            {
                return await Utill.RateLimited(() => JoinLobbyByID(lobbyID, password), 2000);
            }
            catch (LobbyServiceException notfound) when (notfound.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                UtilDebug.Log($"LobbyNotFound{notfound.Message}");
                throw;
            }
            catch (Exception error)
            {
                UtilDebug.Log($"An Error Occured ErrorCode:{error}");
                throw;
            }

            if (preLobby != null)
                await ExitLobbyAsync(preLobby);

            _currentLobby = nextLobby;
            CheckHostAndSendHeartBeat(_currentLobby);
            await JoinLobbyInitalize(_currentLobby, OnRoomLobbyChangeHostEventAsync);
            bool isRelayJoined = await TryJoinClientRelayWithRetry(_currentLobby);
            if (isRelayJoined == false)
            {
                await ExitLobbyAsync(_currentLobby);
                await TryJoinLobbyByNameOrCreateWaitLobby();
                ShowJoinRelayFailedDialog();
                return null;
            }

            return _currentLobby;
        }


        public async UniTask LogoutAndAllLeaveLobby()
        {
            if (AuthenticationService.Instance.IsSignedIn == false)
                return;

            try
            {
                await LeaveAllLobby();
                UtilDebug.Log("Player removed from lobby.");
            }
            catch (LobbyServiceException ex) when (ex.Reason == LobbyExceptionReason.RateLimited)
            {
                UtilDebug.Log($"Failed to remove player from lobby: {ex.Message} 다시 시도중");
                await Utill.RateLimited(LogoutAndAllLeaveLobby);
            }
            catch (Exception ex)
            {
                UtilDebug.Log($"에러발생: {ex}");
            }

            // 사용자 인증 로그아웃
            AuthenticationService.Instance.SignOut();
            AuthenticationService.Instance.ClearSessionToken();
            CurrentPlayerIngameInfo = default;
            _currentLobby = null;
            UtilDebug.Log("User signed out successfully.");
        }

        public void InitializeVivoxEvent()
        {
            _vivoxSession.VivoxDoneLoginEvent -= SetVivoxTaskCheker;
            _vivoxSession.VivoxDoneLoginEvent += SetVivoxTaskCheker;
        }

        public void InitializeLobbyEvent()
        {
            _socketEventManager.LogoutAllLeaveLobbyEvent -= LogoutAndAllLeaveLobby;
            _socketEventManager.LogoutAllLeaveLobbyEvent += LogoutAndAllLeaveLobby;
        }

        public async UniTask ReFreshRoomList()
        {
            await RefreshRoomListPage(GetRoomPageToken(_currentRoomPageIndex), _currentRoomPageIndex);
        }

        public async UniTask NextRoomPage()
        {
            if (_isRefreshing)
                return;

            if (string.IsNullOrEmpty(_nextRoomPageToken))
                return;

            int nextPageIndex = _currentRoomPageIndex + 1;
            SetRoomPageToken(nextPageIndex, _nextRoomPageToken);
            await RefreshRoomListPage(_nextRoomPageToken, nextPageIndex);
        }

        public async UniTask PreviousRoomPage()
        {
            if (_isRefreshing)
                return;

            if (_currentRoomPageIndex <= 0)
                return;

            int previousPageIndex = _currentRoomPageIndex - 1;
            await RefreshRoomListPage(GetRoomPageToken(previousPageIndex), previousPageIndex);
        }

        private async UniTask RefreshRoomListPage(string continuationToken, int pageIndex)
        {
            
            //로비에 없으면 리턴
            if (_currentLobby == null)
                return;

            //이미 새로고침중이거나 RoomInventoryUI가 없으면 중단
            if (_isRefreshing || _uiManager.Try_Get_Scene_UI(out UIRoomInventory room_inventory_ui) == false)
            {
                return;
            }

            _isRefreshing = true;
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions
                {
                    Count = RoomPageSize, //보여줄 1페이지 당 최대 방 갯수
                    SampleResults = false,
                    ContinuationToken = continuationToken,
                    Filters = new List<QueryFilter>()
                    {
                        new QueryFilter(
                            field: QueryFilter.FieldOptions.S1,
                            op: QueryFilter.OpOptions.EQ,
                            value: "CharactorSelect"),
                        new QueryFilter(
                            field: QueryFilter.FieldOptions.S2,
                            op: QueryFilter.OpOptions.EQ,
                            value: "Open"),
                        new QueryFilter(
                            field: QueryFilter.FieldOptions.AvailableSlots,
                            op: QueryFilter.OpOptions.GT,
                            value: "0"),
                    },
                    Order = new List<QueryOrder>()
                    {
                        new QueryOrder(false, QueryOrder.FieldOptions.Created)
                    }
                };
                QueryResponse lobbies = await GetQueryLobbiesAsyncCustom(options);
                _currentRoomPageIndex = pageIndex;
                _nextRoomPageToken = lobbies.ContinuationToken;
                if (string.IsNullOrEmpty(_nextRoomPageToken) == false)
                {
                    SetRoomPageToken(_currentRoomPageIndex + 1, _nextRoomPageToken);
                }

                foreach (Transform child in room_inventory_ui.RoomContent)
                {
                    _resourcesServices.DestroyObject(child.gameObject);
                }

                foreach (Lobby lobby in lobbies.Results)
                {
                    CreateRoomInitalize(lobby);
                }

                //현재 페이지 번호, 이전/다음 버튼 가능 여부를 UI에 반영함.
                room_inventory_ui.SetRoomPageState(
                    CurrentRoomPage,
                    CanMovePreviousRoomPage,
                    CanMoveNextRoomPage);
            }
            catch (LobbyServiceException e)
            {
                UtilDebug.Log($"에러발생:{e}");
                throw;
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private string GetRoomPageToken(int pageIndex)
        {
            return _roomPageTokens[pageIndex];
        }

        private void SetRoomPageToken(int pageIndex, string pageToken)
        {
            while (_roomPageTokens.Count <= pageIndex)
            {
                _roomPageTokens.Add(null);
            }

            _roomPageTokens[pageIndex] = pageToken;
        }

        private void CreateRoomInitalize(Lobby lobby)
        {
            if (_uiManager.Try_Get_Scene_UI(out UIRoomInventory roomInventoryUI) == false)
                return;

            UIRoomInfoPanel infoPanel = _uiManager.MakeSubItem<UIRoomInfoPanel>(roomInventoryUI.RoomContent);
            infoPanel.SetRoomInfo(lobby);
        }

        public async UniTask LoadingPanel(Func<UniTask> process)
        {
            _lobbyLoading?.Invoke(true);
            await process.Invoke();
            _lobbyLoading?.Invoke(false);
        }
        public async UniTask DeleteRelayCodefromLobbyAsync(Lobby lobby)
        {
            if (lobby == null)
                return;

            if (lobby.HostId != PlayerID)
                return;

            // 2026-05-26: WaitLobby는 RelayCode를 사용하지 않으므로 RoomLobby에 RelayCode가 있을 때만 삭제한다.
            if (lobby.Data == null || lobby.Data.ContainsKey("RelayCode") == false)
                return;

            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", null }
                }
            });
        }
        #region TestDebugCode

        public void SetPlayerLoginInfo(PlayerIngameLoginInfo info)
        {
            CurrentPlayerIngameInfo = info;
        }

        public void ShowLobbyData()
        {
            foreach (var data in _currentLobby.Data)
            {
                UtilDebug.Log($"{data.Key}의 값은 {data.Value.Value}");
            }
        }
        public async UniTask ShowUpdatedLobbyPlayers()
        {
            try
            {
                QueryResponse lobbies = await GetQueryLobbiesAsyncCustom();
                foreach (var lobby in lobbies.Results)
                {
                    Player hostPlayer =
                        lobby.Players.FirstOrDefault(player => player.Id == lobby.HostId);

                    UtilDebug.Log(
                        $"현재 로비이름: {lobby.Name} 로비ID: {lobby.Id} 호스트닉네임: {hostPlayer.Data["NickName"].Value} 로비호스트: {lobby.HostId} ");
                    UtilDebug.Log($"-----------------------------------");
                    foreach (var player in lobby.Players)
                    {
                        UtilDebug.Log($"플레이어 아이디: {player.Id} 플레이어 닉네임:{player.Data["NickName"].Value}");
                    }

                    UtilDebug.Log($"-----------------------------------");
                }
            }
            catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.RequestTimeOut)
            {
                UtilDebug.LogError($"RequestTimeOut");
                await Utill.RateLimited(async () => { await ShowUpdatedLobbyPlayers(); });
            }
            catch (Exception ex)
            {
                UtilDebug.Log($"에러{ex}");
            }
        }

        #endregion

    }
}
