namespace GameManagers.NGOPoolManagement
{
    public interface INgoPooldata 
    {
        public string PoolingNgoPath { get; }
        public int PoolingCapacity { get; }
        public bool PreloadOnSceneEnter { get; }
    }
}
