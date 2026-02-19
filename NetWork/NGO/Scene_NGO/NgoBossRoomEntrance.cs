using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using NetWork.BaseNGO;
using Stats;
using UI.WorldSpace;
using UI.WorldSpace.PortalIndicator;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.Scene_NGO
{
    public readonly struct BossRoomEntrancePosition
    {
        public BossRoomEntrancePosition(Vector3 position)
        {
            TownPortalPosition = position;
        }
        public Vector3 TownPortalPosition { get; }
    }
    
    
    public class NgoBossRoomEntrance : NetworkBehaviourBase
    {
        public class NgoBossRoomEntranceFactory : NgoZenjectFactory<NgoBossRoomEntrance>
        {
            public NgoBossRoomEntranceFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/Scene_NGO/NGOBossRoomEntrance");
            }
        }
        [Inject] 
        public void Construct(RelayManager relayManager,BossRoomEntrancePosition entrancePosition)
        {
            _relayManager = relayManager;
            _entrancePosition = entrancePosition;
        }
        
        private RelayManager _relayManager;
        private BossRoomEntrancePosition _entrancePosition; 
        // 이 값은 단순 위치 벡터값만 있음. 나중에 테스트할때 포탈위치를 옮기고 싶다면 테스트 인스톨러에 ReBind해서 위치를 바꾸고 사용할 것;
        
        private NgoStageTimerController _timerController;
        public NgoStageTimerController TimerController
        {
            get
            {
                if(_timerController == null)
                {
                
                    foreach(NetworkObject ngo in _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjectsList)
                    {
                        if(ngo.TryGetComponent(out NgoStageTimerController stageTimerController))
                        {
                            _timerController = stageTimerController;
                            break;
                        }
                    }
                }
                return _timerController;
            }
        }

        NetworkVariable<int> _playerCountInPortal = new NetworkVariable<int>
            (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        NetworkVariable<bool> _isAllplayersinPortal = new NetworkVariable<bool>
            (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsHost == false)
                return;

            gameObject.transform.position = _entrancePosition.TownPortalPosition;


            _playerCountInPortal.OnValueChanged -= OnChangedCountPlayer;
            _playerCountInPortal.OnValueChanged += OnChangedCountPlayer;
        }



        private void OnChangedCountPlayer(int previousValue, int newValue)
        {

            if(newValue == _relayManager.NetworkManagerEx.ConnectedClientsList.Count)
            {
                _isAllplayersinPortal.Value = true;
                TimerController.SetPortalInAllPlayersCountRpc();
            }
            else
            {
                if (_isAllplayersinPortal.Value == false)
                    return;

                _isAllplayersinPortal.Value = false;
                TimerController.SetNormalCountRpc();
            }
        }

        protected override void StartInit()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsHost == false)
                return;
            

            if (other.transform.TryGetComponentInParents(out PlayerStats playerStats) == true)
            {
                _playerCountInPortal.Value++;

                if (playerStats.TryGetComponent(out NetworkObject playerNgo))
                {
                    EnteredPlayerInPortalRpc(playerNgo.NetworkObjectId);
                }
            }
        }


        private void OnTriggerExit(Collider other)
        {
            if (IsHost == false)
                return;

            if (other.transform.TryGetComponentInParents(out PlayerStats playerStats) == true)
            {
                _playerCountInPortal.Value--;
                if (playerStats.TryGetComponent(out NetworkObject playergo))
                {
                    ExitedPlayerInPortalRpc(playergo.NetworkObjectId);
                }
            }
        }

        protected override void AwakeInit()
        {
        }



        [Rpc(SendTo.ClientsAndHost)]
        public void EnteredPlayerInPortalRpc(ulong playerIndex)
        {
            if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(playerIndex,out NetworkObject player))
            {
                player.gameObject.TryGetComponentInChildren(out UIPortalIndicator indicator);
                indicator.SetIndicatorOn();
            }
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void ExitedPlayerInPortalRpc(ulong playerIndex)
        {
            if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(playerIndex, out NetworkObject player))
            {
                player.gameObject.TryGetComponentInChildren(out UIPortalIndicator indicator);
                indicator.SetIndicatorOff();
            }
        }
    }
}
