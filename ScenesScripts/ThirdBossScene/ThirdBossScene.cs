using NetWork.NGO.Scene_NGO;
using ScenesScripts.CommonInstaller.Interfaces;
using Util;
using Zenject;

namespace ScenesScripts.ThirdBossScene
{
    public class ThirdBossScene : BaseScene, ISkillInit, IHasSceneMover, IHasSpawnPosition
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

        public override Define.SceneName CurrentSceneName => Define.SceneName.ThirdBossScene;

        protected override void StartInit()
        {
            base.StartInit();
            _sceneStarter.SceneStart();
        }
    }
}
