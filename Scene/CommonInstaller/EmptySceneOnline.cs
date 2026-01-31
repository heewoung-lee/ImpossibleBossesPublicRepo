using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Scene.CommonInstaller.Interfaces;

namespace Scene.CommonInstaller
{
    public class EmptySceneOnline : ISceneConnectOnline
    {
        public UniTask SceneConnectOnlineStart()
        {
            return UniTask.CompletedTask;
        }
    }
}
