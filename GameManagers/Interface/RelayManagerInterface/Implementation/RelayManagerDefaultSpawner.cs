using GameManagers.Interface.ResourcesManager;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.RelayManagerInterface.Implementation
{
    public class RelayManagerDefaultSpawner : IRelayManagerSpawnObject
    {
        private readonly IResourcesServices _resourcesServices;

        [Inject]
        public RelayManagerDefaultSpawner(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }

        public void ScheduleSpawnAfterInit(RelayManager relayManager)
        {
            SpawnRPCCaller(relayManager);
        }
        private void SpawnRPCCaller(RelayManager relayManager)
        {
            if (relayManager.NetworkManagerEx.IsHost == false)
                return;

            if (relayManager.NgoRPCCaller != null)
                return;

            GameObject ngo = _resourcesServices.InstantiateByKey("Prefabs/NGO/NgoRPCCaller");
            relayManager.SpawnNetworkObj(ngo, destroyOption: false);
        }
    }
}