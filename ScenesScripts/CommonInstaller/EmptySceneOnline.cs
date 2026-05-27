using Cysharp.Threading.Tasks;
using ScenesScripts.CommonInstaller.Interfaces;

namespace ScenesScripts.CommonInstaller
{
    public class EmptySceneOnline : ISceneConnectOnline
    {
        public UniTask SceneConnectOnlineStart()
        {
            return UniTask.CompletedTask;
        }
    }
}
