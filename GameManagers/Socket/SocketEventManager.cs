using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Util;

namespace GameManagers
{
    public class SocketEventManager
    {
        private Func<UniTask> _disconnectRelayEvent;
        private Func<UniTask> _logoutVivoxEvent;
        private Func<UniTask> _logoutAllLeaveLobbyEvent;

        public event Func<UniTask> DisconnectRelayEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _disconnectRelayEvent, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _disconnectRelayEvent, value);
            }
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


        public UniTask InvokeDisconnectRelayEvent() => _disconnectRelayEvent?.Invoke() ?? UniTask.CompletedTask;
        public UniTask InvokeLogoutVivoxEvent() => _logoutVivoxEvent?.Invoke() ?? UniTask.CompletedTask;
        public UniTask InvokeLogoutAllLeaveLobbyEvent() => _logoutAllLeaveLobbyEvent?.Invoke() ?? UniTask.CompletedTask;
        
        
        
        
    }
}
