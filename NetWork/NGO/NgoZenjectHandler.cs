using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using Zenject;

namespace NetWork.NGO
{
    
    /// <summary>
    /// NgoZenjectHandler는 현재 ProjectContext에 바인드 되어있음.
    /// </summary>
    public class NgoZenjectHandler: INetworkPrefabInstanceHandler
    {
        private readonly DiContainer _diContainer;
        private readonly GameObject _prefab;
        private readonly IResourcesServices _resourcesServices;
        
        public class NgoZenjectHandlerFactory: PlaceholderFactory<DiContainer,GameObject, NgoZenjectHandler> { }
        public NgoZenjectHandler(DiContainer diContainer, GameObject prefab,IResourcesServices resourcesServices)
        {
            _diContainer = diContainer;
            _prefab = prefab;
            _resourcesServices = resourcesServices;
        }
        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            GameObject networkObj = Object.Instantiate(_prefab, position, rotation);
            _diContainer.InjectGameObject(networkObj);
            return networkObj.GetComponent<NetworkObject>();
        }
        public void Destroy(NetworkObject networkObject)
        {
            _resourcesServices.DestroyObject(networkObject.gameObject);
        }
    }
}
