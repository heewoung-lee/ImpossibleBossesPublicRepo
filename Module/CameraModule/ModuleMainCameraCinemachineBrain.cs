using GameManagers;
using GameManagers.Interface.ResourcesManager;
using UnityEngine;
using Util;
using Zenject;

namespace Module.CameraModule
{
    public class ModuleMainCameraCinemachineBrain : MonoBehaviour
    {
        private IResourcesServices _resourcesServices;
        
        [Inject] 
        public void Construct(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }
        
        GameObject _mainCamera;
        void Start()
        {
            _mainCamera = GameObject.Find("CinemachineBrainCamera") == true ? GameObject.Find("CinemachineBrainCamera") :
                _resourcesServices.InstantiateByKey("Prefabs/Camera/CinemachineBrainCamera");

            _resourcesServices.GetOrAddComponent<ModuleCallToFollwingCamera>(_mainCamera);
        }
    }
}
