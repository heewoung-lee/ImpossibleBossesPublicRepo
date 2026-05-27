using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameManagers.LoginManagement;
using GameManagers.SocketManagement;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Util;
using Zenject;

namespace GameManagers.VivoxManagement
{
    public class VivoxManager : IManagerEventInitialize,ISendMessage,IVivoxSession
    {
        [Inject] IPlayerIngameLogininfo _playerIngameLogininfo;
        [Inject] SocketEventManager _socketEventManager;
        
        private Action _vivoxDoneLoginEvent;
        public event Action VivoxDoneLoginEvent
        {
            add
            {
               UniqueEventRegister.AddSingleEvent(ref _vivoxDoneLoginEvent, value);
            }
            remove
            {
               UniqueEventRegister.RemovedEvent(ref _vivoxDoneLoginEvent, value);
            }
        }
        private bool _checkDoneLoginProcess = false;
        public bool CheckDoneLoginProcess => _checkDoneLoginProcess;
        private LoginOptions _loginOptions;
        private string _currentChanel = null;
        public string CurrentChannel => _currentChanel;
        public PlayerIngameLoginInfo CurrentPlayerInfo => _playerIngameLogininfo.GetPlayerIngameLoginInfo();
        public async UniTask JoinChannelAsync(string chanelID)
        {
            try
            {
                if (VivoxService.Instance.IsLoggedIn == false)
                {
                    await InitializeAsync();
                }
                if (_currentChanel == chanelID && HasActiveChannel(chanelID))
                    return;

                if (_currentChanel != null)
                {
                    UtilDebug.Log($"Vivox 채널{_currentChanel}지워짐");
                    await LeaveEchoChannelAsyncCustom(_currentChanel);
                    _currentChanel = null;
                }

                await JoinGroupChannelAsyncCustom(chanelID, ChatCapability.TextOnly);
                _currentChanel = chanelID;
            }
            catch (RequestFailedException requestFailExceoption)
            {
                UtilDebug.Log($"오류발생{requestFailExceoption}");
                await Utill.RateLimited(async ()=> await JoinChannelAsync(chanelID));
            }
            catch(ArgumentException alreadyAddKey) when (alreadyAddKey.Message.Contains("An item with the same key has already been added"))
            {
                UtilDebug.Log($"{alreadyAddKey}이미 키가 있음 무시해도 됨");
            }
            catch (Exception ex) 
            {
                UtilDebug.LogError($"JoinChannel 에러 발생{ex}");
                throw;
            }

        }
        public async UniTask LogoutOfVivoxAsync()
        {
            try
            {
                UtilDebug.Log("vivox 로그아웃");
                await VivoxService.Instance.LogoutAsync();
                _checkDoneLoginProcess = false;
                _currentChanel = null;
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"LogoutOfVivoxAsync 에러 발생{ex}");
                throw;
            }
        }
        public async UniTask SendSystemMessageAsync(string systemMessage)
        {
            try
            {
                if (VivoxService.Instance.IsLoggedIn == false || HasActiveChannel(_currentChanel) == false)
                    return;

                string formattedMessage = $"<color=#FFD700>[SYSTEM]</color> {systemMessage}";
                await VivoxService.Instance.SendChannelTextMessageAsync(_currentChanel, formattedMessage);
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"SendSystemMessageAsync error:{ex}");
                throw;
            }
        }
        public async UniTask SendMessageAsync(string message)
        {
            try
            {
                if (VivoxService.Instance.IsLoggedIn == false || HasActiveChannel(_currentChanel) == false)
                    return;

                string sendMessageFormmat = $"[{_loginOptions.DisplayName}] {message}";
                await VivoxService.Instance.SendChannelTextMessageAsync(_currentChanel, sendMessageFormmat);
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"SendMessageAsync 에러 발생{ex}");
                throw;
            }
        }
        private async UniTask InitializeAsync()
        {
            try
            {
                InitializeVivoxEvent();
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                await VivoxService.Instance.InitializeAsync();
                await LoginToVivoxAsync();
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"InitializeAsync 에러 발생{ex}");
                throw;
            }
        }
        private async UniTask LoginToVivoxAsync()
        {
            if (VivoxService.Instance.IsLoggedIn)
            {
                UtilDebug.Log("로그인이 되어있음 리턴하겠음");
                return;

            }

            try
            {
                _loginOptions = new LoginOptions();
                _loginOptions.DisplayName = CurrentPlayerInfo.PlayerNickName;
                _loginOptions.EnableTTS = true;
                await VivoxService.Instance.LoginAsync(_loginOptions);
                _checkDoneLoginProcess = true;
                _vivoxDoneLoginEvent?.Invoke();
                UtilDebug.Log("ViVox 로그인완료");
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"LoginToVivoxAsync 에러 발생{ex}");
                throw;
            }
        }
        private async UniTask JoinGroupChannelAsyncCustom(string currentChanel,ChatCapability chatCapbillty)
        {
            try
            {
                //await VivoxService.Instance.LeaveAllChannelsAsync();
                await VivoxService.Instance.JoinGroupChannelAsync(currentChanel, chatCapbillty);
            }
            catch (Exception e) when (e.Message.Contains("Request timeout"))
            {
                UtilDebug.LogError($"LeaveEchoChannelAsync 에러 발생{e}");
                await Utill.RateLimited(async () => await VivoxService.Instance.JoinGroupChannelAsync(currentChanel, chatCapbillty));
                throw;
            }
            catch(Exception authorizedException) when (authorizedException.Message.Contains("not authorized"))
            {
                await ReLoginToVivoxAsync();
                await VivoxService.Instance.JoinGroupChannelAsync(currentChanel, chatCapbillty);
            }
            catch (Exception error)
            {
                UtilDebug.LogError($"에러발생{error}");
                throw;
            }
        }
        private async UniTask LeaveEchoChannelAsyncCustom(string chanelID)
        {
            try
            {
                //await VivoxService.Instance.LeaveAllChannelsAsync();
                if(VivoxService.Instance.ActiveChannels.ContainsKey(chanelID) != default)
                    await VivoxService.Instance.LeaveChannelAsync(chanelID);
            }
            catch (Exception e) when (e.Message.Contains("Request timeout"))
            {
                UtilDebug.LogError($"LeaveEchoChannelAsync 에러 발생{e}");
                await Utill.RateLimited(async () => await LeaveEchoChannelAsyncCustom(chanelID));
                throw;
            }
            catch(Exception error)
            {
                UtilDebug.LogError($"에러발생{error}");
                throw;
            }
        }

        private bool HasActiveChannel(string chanelID)
        {
            return string.IsNullOrEmpty(chanelID) == false && VivoxService.Instance.ActiveChannels.ContainsKey(chanelID);
        }

        private async UniTask ReLoginToVivoxAsync()
        {
            if (VivoxService.Instance.IsLoggedIn)
            {
                await VivoxService.Instance.LogoutAsync();
                _checkDoneLoginProcess = false;
                _currentChanel = null;
            }

            await LoginToVivoxAsync();
        }

        public void InitializeVivoxEvent()
        {
            _socketEventManager.LogoutVivoxEvent += LogoutOfVivoxAsync;
        }
    }
}
