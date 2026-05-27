using Cysharp.Threading.Tasks;

namespace GameManagers.VivoxManagement
{
    public interface ISendMessage
    {
        public UniTask SendSystemMessageAsync(string systemMessage);
        public UniTask SendMessageAsync(string message);
    }
}
