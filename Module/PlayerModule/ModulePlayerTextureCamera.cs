using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using UnityEngine;
using Zenject;

namespace Module.PlayerModule
{
    public class ModulePlayerTextureCamera : MonoBehaviour
    {
        [Inject] private IResourcesServices _resourcesServices;
        private GameObject _playerTextureCamara;
        void Start()
        {
            _playerTextureCamara = _resourcesServices.InstantiateByKey("Prefabs/Player/PlayerInvenTextureCamera", transform);
        }
    }
}
