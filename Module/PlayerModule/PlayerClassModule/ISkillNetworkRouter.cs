

using UnityEngine;

namespace Module.PlayerModule.PlayerClassModule
{
    public interface IChainVfxHandler
    {
        void SetChainData(
            ulong startNetId, 
            ulong endNetId, 
            Vector3 startOffset, 
            Vector3 endOffset, 
            float duration
        );
    }
    
    public interface ISkillNetworkRouter
    {
        public void RequestSpawnChainSkillServerRpc(
            string prefabPath,
            ulong startNetId,
            ulong endNetId,
            Vector3 startOffset,
            Vector3 endOffset,
            float duration);
    }
}