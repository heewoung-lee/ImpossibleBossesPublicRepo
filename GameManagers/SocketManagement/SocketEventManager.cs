using System;
using Cysharp.Threading.Tasks;
using GameManagers.RelayManagement;
using Util;

namespace GameManagers.SocketManagement
{
    public class SocketEventManager
    {
        private Func<RelayDisconnectCause, UniTask> _disconnectRelayEvent;
        private Func<UniTask> _logoutVivoxEvent;
        private Func<UniTask> _logoutAllLeaveLobbyEvent;

        public event Func<RelayDisconnectCause, UniTask> DisconnectRelayEvent
        {
            add { UniqueEventRegister.AddSingleEvent(ref _disconnectRelayEvent, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _disconnectRelayEvent, value); }
        }


        public event Func<UniTask> LogoutVivoxEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _logoutVivoxEvent, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _logoutVivoxEvent, value);
            }
        }

        public event Func<UniTask> LogoutAllLeaveLobbyEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _logoutAllLeaveLobbyEvent, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _logoutAllLeaveLobbyEvent, value);
            }
        }


        public UniTask InvokeDisconnectRelayEvent(RelayDisconnectCause cause)
        {
            if (_disconnectRelayEvent == null)
                return UniTask.CompletedTask;

            return _disconnectRelayEvent.Invoke(cause);
        }
        public UniTask InvokeLogoutVivoxEvent() => _logoutVivoxEvent?.Invoke() ?? UniTask.CompletedTask;
        public UniTask InvokeLogoutAllLeaveLobbyEvent() => _logoutAllLeaveLobbyEvent?.Invoke() ?? UniTask.CompletedTask;
        
        
        
        
    }
}
