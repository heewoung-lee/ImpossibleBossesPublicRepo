using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace GameManagers.Interface.VivoxManager
{
    public interface IVivoxSession
    {
        public UniTask JoinChannelAsync(string chanelID);
        public UniTask LogoutOfVivoxAsync();
        public bool CheckDoneLoginProcess { get; }
        public event Action VivoxDoneLoginEvent;
    }
}
