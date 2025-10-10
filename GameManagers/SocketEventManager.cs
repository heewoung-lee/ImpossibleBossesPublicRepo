using System;
using System.Threading.Tasks;
using UnityEngine;
using Util;

namespace GameManagers
{
    public class SocketEventManager
    {
        private Func<Task> _disconnectRelayEvent;
        private Func<Task> _logoutVivoxEvent;
        private Func<Task> _logoutAllLeaveLobbyEvent;

        public event Func<Task> DisconnectRelayEvent
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

        public event Func<Task> LogoutVivoxEvent
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

        public event Func<Task> LogoutAllLeaveLobbyEvent
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


        public Task InvokeDisconnectRelayEvent() => _disconnectRelayEvent?.Invoke() ?? Task.CompletedTask;
        public Task InvokeLogoutVivoxEvent() => _logoutVivoxEvent?.Invoke() ?? Task.CompletedTask;
        public Task InvokeLogoutAllLeaveLobbyEvent() => _logoutAllLeaveLobbyEvent?.Invoke() ?? Task.CompletedTask;
        
        
        
        
    }
}
