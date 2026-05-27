namespace ScenesScripts.CommonInstaller.Interfaces
{
    public interface IRegistrar<in T>
    {
        public void Register(T sceneContext);
        public void Unregister(T sceneContext);
    }
}
