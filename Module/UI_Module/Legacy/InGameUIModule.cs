using Controller;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using UnityEngine;
using Zenject;

namespace Module.UI_Module
{
    public class InGameUIModule : MonoBehaviour
    {
         private IResourcesServices _resourcesServices;
        
        [Inject]
        public void Construct(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }
        
        private void Start()
        {
            StartInit();
        }

        protected virtual void StartInit()
        {
            //_resourcesServices.GetOrAddComponent<ModuleUIBufferBar>(gameObject);
            //_resourcesServices.GetOrAddComponent<ModuleUIConsumableBar>(gameObject);
            //_resourcesServices.GetOrAddComponent<ModuleUIItemDragImage>(gameObject);
            //_resourcesServices.GetOrAddComponent<ModuleUIPlayerInfo>(gameObject);
            
           // _resourcesServices.GetOrAddComponent<UIPlayerInventoryController>(gameObject);
            //_resourcesServices.GetOrAddComponent<UISkillBarController>(gameObject);
            //_resourcesServices.GetOrAddComponent<UIDescriptionController>(gameObject);
            //_resourcesServices.GetOrAddComponent<MoveMarkerController>(gameObject);
        }
    }
}
