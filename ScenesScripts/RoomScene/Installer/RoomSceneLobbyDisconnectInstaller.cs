using GameManagers.LobbyManagement;
using Zenject;

namespace ScenesScripts.RoomScene.Installer
{
    /// <summary>
    /// 2.28일 추가 룸씬에서는 호스트가 나가면 각 클라이언트끼리 호스트 마이그레이션 작업을 해야하기에
    /// 전부 로비로 돌아가면 안됨 그래서 연결이 끊어졌을때의 구현부를 다르게 해,
    /// 룸씬에서는 연결끊기를 RoomMigrationStrategy이걸 수행하도록 하게 만듦
    /// </summary>
    public class RoomSceneLobbyDisconnectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RoomMigrationStrategy>().AsSingle();
        }
    }
}
