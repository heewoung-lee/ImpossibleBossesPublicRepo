using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIManager;
using UI.Scene.SceneUI;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.Scene_NGO
{
    public class NgoMoveDownTownBehaviour : NetworkBehaviour
    {
        public class NgoMoveDownTownBehaviourFactory : NgoZenjectFactory<NgoMoveDownTownBehaviour>
        {
            public NgoMoveDownTownBehaviourFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NGO_MoveDownTownBehaviour");
            }
        }

        private IUIManagerServices _uiManager;
        private IResourcesServices _resourcesServices;

        [Inject]
        public void Construct(IUIManagerServices uiManager, IResourcesServices resourcesServices)
        {
            _uiManager = uiManager;
            _resourcesServices = resourcesServices;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (_uiManager.Try_Get_Scene_UI(out UIBossHp bossHp))
            {
                _resourcesServices.DestroyObject(bossHp.gameObject);
            }
        }
    }
}