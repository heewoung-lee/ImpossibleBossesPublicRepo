namespace ScenesScripts.CommonInstaller.Interfaces
{
    public interface ISceneOpeningCinematicCoordinator
    {
        void OnLoadingComplete();
    }

    public interface IBossSpawnCinematicTarget
    {
        void OnSpawnedForCinematic();
        void OnCinematicStart();
        void OnCombatStart();
    }
}
