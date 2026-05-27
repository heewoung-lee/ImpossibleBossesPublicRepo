using System;
using Cysharp.Threading.Tasks;

namespace GameManagers.VivoxManagement
{
    public interface IVivoxSession
    {
        public UniTask JoinChannelAsync(string chanelID);
        public UniTask LogoutOfVivoxAsync();
        public bool CheckDoneLoginProcess { get; }
        public string CurrentChannel { get; }
        public event Action VivoxDoneLoginEvent;
    }
}
