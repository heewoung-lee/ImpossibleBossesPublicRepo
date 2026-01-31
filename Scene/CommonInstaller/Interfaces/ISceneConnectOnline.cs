using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Scene.CommonInstaller.Interfaces
{
    public enum PlayersTag
    {
        Player1,
        Player2,
        Player3,
        Player4,
        None
    }
    
    public interface ISceneConnectOnline
    { 
        public UniTask SceneConnectOnlineStart();
    }
}
