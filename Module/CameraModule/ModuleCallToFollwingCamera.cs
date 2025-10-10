using Controller;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using UnityEngine;
using Util;
using Zenject;

namespace Module.CameraModule
{
    public class ModuleCallToFollwingCamera : MonoBehaviour
    {
        private GameObject _playerFollwingCamera;
        [Inject] IResourcesServices _resourcesServices;
        void Start()
        {
            _playerFollwingCamera = GameObject.Find("PlayerFollowingCamera") == true ? 
                GameObject.Find("PlayerFollowingCamera") : _resourcesServices.InstantiateByKey("Prefabs/Camera/PlayerFollowingCamera");
            _resourcesServices.GetOrAddComponent<PlayerFollowingCamera>(_playerFollwingCamera);
        }

    }
}
