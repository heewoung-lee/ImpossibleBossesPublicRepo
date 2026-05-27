using ScenesScripts.CommonInstaller.Interfaces;
using GameManagers.RelayManagement;
using GameManagers.SceneManagement;
using NetWork.NGO.Scene_NGO;
using Util;
using Zenject;

namespace ScenesScripts.SecondBossScene
{
    public class SecondBossScene : BaseScene, ISkillInit, IHasSceneMover, IHasSpawnPosition
    {
        private ISceneStarter _sceneStarter;
        private ISceneMover _sceneMover;
        private SpawnPosition _spawnPosition;

        [Inject]
        public void Construct(
            ISceneStarter gameplaySceneStarter,
            ISceneMover sceneMover,
            SpawnPosition spawnPosition)
        {
            _sceneStarter = gameplaySceneStarter;
            _sceneMover = sceneMover;
            _spawnPosition = spawnPosition;
        }

        public ISceneMover SceneMover => _sceneMover;
        public SpawnPosition SpawnPosition => _spawnPosition;

        public override Define.SceneName CurrentSceneName => Define.SceneName.SecondBossScene;

        protected override void StartInit()
        {
            base.StartInit();
            _sceneStarter.SceneStart();
        }

    }

    public class SecondBossSceneMover : ISceneMover
    {
        private readonly BaseScene _baseScene;
        private readonly SceneManagerEx _sceneManagerEx;
        private readonly RelayManager _relayManager;

        [Inject]
        public SecondBossSceneMover(
            BaseScene baseScene,
            SceneManagerEx sceneManagerEx,
            RelayManager relayManager)
        {
            _baseScene = baseScene;
            _sceneManagerEx = sceneManagerEx;
            _relayManager = relayManager;
        }

        public void MoveScene()
        {
            if (_relayManager.NetworkManagerEx.IsHost == false)
                return;

            Define.SceneName nextScene = _sceneManagerEx.GetNextSceneByFlow(_baseScene.CurrentSceneName);

            _relayManager.NetworkManagerEx.NetworkConfig.EnableSceneManagement = true;
            _sceneManagerEx.NetworkLoadScene(nextScene);
            _relayManager.NgoRPCCaller.ResetManagersRpc();
        }
    }
}
