using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIManager;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using UI.Scene.SceneUI;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.UI
{
    public class NgoUIRootCharacterSelect : NetworkBehaviour
    {
        private IUIManagerServices _uiManagerServices;
        private RelayManager _relayManager;


        [Inject]
        public void Construct(IUIManagerServices uiManagerServices, RelayManager relayManager)
        {
            _uiManagerServices = uiManagerServices;
            _relayManager = relayManager;
        }


        public class NgoUIRootCharacterSelectFactory : NgoZenjectFactory<NgoUIRootCharacterSelect>
        {
            public NgoUIRootCharacterSelectFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
            _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NGOUIRootChracterSelect");
            }

        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost == false)
                return;

            transform.SetParent(_relayManager.NgoRootUI.transform);
            _uiManagerServices.Get_Scene_UI<UIRoomCharacterSelect>().Set_NGO_UI_Root_Character_Select(this.transform);
        }
    }
}