using ScenesScripts.CommonInstaller.Interfaces;
using NetWork.NGO.Scene_NGO;
using Util;
using Zenject;

namespace ScenesScripts.FirstBossScene
{
    public class FirstBossScene : BaseScene, ISkillInit, IHasSceneMover, IHasSpawnPosition
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

        public override Define.SceneName CurrentSceneName => Define.SceneName.FirstBossScene;

        protected override void StartInit()
        {
            base.StartInit();
            _sceneStarter.SceneStart();
        }

    }
}
