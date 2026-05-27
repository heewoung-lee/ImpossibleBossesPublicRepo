using System.Collections.Generic;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using UI.Scene.SceneUI;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.UI
{
    public class NgoUIRootCharacterSelect : NetworkBehaviour
    {
        private const int  MAX_PLAYER = 8;
        
        private IUIManagerServices _uiManagerServices;
        private RelayManager _relayManager;
        
        private bool[] _occupiedSlots = new bool[MAX_PLAYER];
        private Dictionary<ulong, int> _clientSlotMaps = new Dictionary<ulong, int>();


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

            transform.SetParent(_relayManager.NgoRootUI.transform,false);
            _uiManagerServices.Get_Scene_UI<UIRoomCharacterSelect>().Set_NGO_UI_Root_Character_Select(this.transform);
        }
        public int AllocateSlot(ulong clientId)
        {
            for (int i = 0; i < MAX_PLAYER; i++)
            {
                if (_occupiedSlots[i] == false) // 비어있는 자리 발견
                {
                    _occupiedSlots[i] = true;
                    _clientSlotMaps[clientId] = i;
                    return i;
                }
            }
            return -1; // 방이 꽉 참
        }
        
        
        public void LeaveSlot(ulong clientId)
        {
            if (_clientSlotMaps.TryGetValue(clientId, out int slotIndex))
            {
                _occupiedSlots[slotIndex] = false;
                _clientSlotMaps.Remove(clientId);
            }
        }
        
    }
}