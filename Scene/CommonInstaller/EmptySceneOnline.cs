using System.Threading.Tasks;
using Scene.CommonInstaller.Interfaces;

namespace Scene.CommonInstaller
{
    public class EmptySceneOnline : ISceneConnectOnline
    {
        public Task SceneConnectOnlineStart()
        {
            return Task.CompletedTask;
        }
    }
}
