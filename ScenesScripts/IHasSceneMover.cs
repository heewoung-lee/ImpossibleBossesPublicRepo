
using NetWork.NGO.Scene_NGO;

namespace ScenesScripts
{
    public interface IHasSceneMover
    {
        public ISceneMover SceneMover { get; }
    }

    public interface IHasSpawnPosition
    {
        public SpawnPosition SpawnPosition { get; }
    }
}
