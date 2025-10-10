using GameManagers.Interface.ResourcesManager;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.RelayManagerInterface.Implementation
{
    public class RelayManagerSpawnTools : IRelayManagerSpawnObject
    {
        private readonly IResourcesServices _resourcesServices;

        [Inject]
        public RelayManagerSpawnTools(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }

        public void ScheduleSpawnAfterInit(RelayManager relayManager)
        {
            SpawnRPCCaller(relayManager);
            SpawnRPCSpawner(relayManager);
        }
        private void SpawnRPCCaller(RelayManager relayManager)
        {
            if (relayManager.NetworkManagerEx.IsHost == false)
                return;

            if (relayManager.NgoRPCCaller != null)
                return;

            NetworkObject ngo = _resourcesServices.InstantiateByKey("Prefabs/NGO/NgoRPCCaller").GetComponent<NetworkObject>();
            ngo.SpawnWithObservers = false;
            relayManager.SpawnNetworkObj(ngo.gameObject, destroyOption: false);
        }

        private void SpawnRPCSpawner(RelayManager relayManager)
        {
            if (relayManager.NetworkManagerEx.IsHost == false)
                return;

            
            NetworkObject ngo = _resourcesServices.InstantiateByKey("Prefabs/NGO/NgoRPCSpawnController").GetComponent<NetworkObject>();
            ngo.SpawnWithObservers = false;
            relayManager.SpawnNetworkObj(ngo.gameObject, destroyOption: false);
        }
    }
}