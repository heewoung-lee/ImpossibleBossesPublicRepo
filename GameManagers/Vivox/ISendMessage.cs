using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace GameManagers.Interface.VivoxManager
{
    public interface ISendMessage
    {
        public UniTask SendSystemMessageAsync(string systemMessage);
        public UniTask SendMessageAsync(string message);
    }
}
