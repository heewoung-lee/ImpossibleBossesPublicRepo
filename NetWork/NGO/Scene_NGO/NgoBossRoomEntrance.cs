using System.Collections.Generic;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using Stats;
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

    public struct SpawnPosition : INetworkSerializable
    {
        public SpawnPosition(Vector3 bossSpawnPosition, Vector3 playerSpawnPosition)
        {
            BossSpawnPosition = bossSpawnPosition;
            PlayerSpawnPosition = playerSpawnPosition;
        }

        public Vector3 BossSpawnPosition { get; private set; }
        public Vector3 PlayerSpawnPosition { get; private set; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            Vector3 bossSpawnPosition = BossSpawnPosition;
            Vector3 playerSpawnPosition = PlayerSpawnPosition;

            serializer.SerializeValue(ref bossSpawnPosition);
            serializer.SerializeValue(ref playerSpawnPosition);

            BossSpawnPosition = bossSpawnPosition;
            PlayerSpawnPosition = playerSpawnPosition;
        }
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
        
        private readonly HashSet<ulong> _playersInPortal = new HashSet<ulong>();
        private readonly List<ulong> _exitedPlayerIds = new List<ulong>();
        private CapsuleCollider _portalCollider;
        
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

            _portalCollider = GetComponent<CapsuleCollider>();
            gameObject.transform.position = _entrancePosition.TownPortalPosition;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _playersInPortal.Clear();
            _exitedPlayerIds.Clear();
        }

        private void Update()
        {
            if (IsHost == false)
                return;

            RefreshPlayersInPortal();
        }

        protected override void StartInit()
        {
        }

        private void RefreshPlayersInPortal()
        {
            if (_portalCollider == null)
                return;

            _exitedPlayerIds.Clear();
            foreach (ulong playerId in _playersInPortal)
            {
                _exitedPlayerIds.Add(playerId);
            }

            int playerCount = 0;
            int alivePlayerCount = 0;
            foreach (NetworkObject playerNgo in _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjectsList)
            {
                if (playerNgo.TryGetComponent(out PlayerStats playerStats) == false)
                    continue;

                if (playerStats.IsDead)
                    continue;

                alivePlayerCount++;

                CapsuleCollider playerCollider = playerStats.GetComponent<CapsuleCollider>();
                if (playerCollider == null || playerCollider.enabled == false)
                    continue;

                if (_portalCollider.bounds.Intersects(playerCollider.bounds) == false)
                    continue;

                playerCount++;
                _exitedPlayerIds.Remove(playerNgo.NetworkObjectId);

                if (_playersInPortal.Add(playerNgo.NetworkObjectId))
                {
                    EnteredPlayerInPortalRpc(playerNgo.NetworkObjectId);
                }
            }

            for (int i = 0; i < _exitedPlayerIds.Count; i++)
            {
                ulong exitedPlayerId = _exitedPlayerIds[i];
                _playersInPortal.Remove(exitedPlayerId);
                ExitedPlayerInPortalRpc(exitedPlayerId);
            }

            if (_playerCountInPortal.Value != playerCount)
            {
                _playerCountInPortal.Value = playerCount;
            }

            UpdatePortalState(playerCount, alivePlayerCount);
        }

        private void UpdatePortalState(int playerCountInPortal, int alivePlayerCount)
        {
            bool isAllAlivePlayersInPortal = alivePlayerCount > 0 && playerCountInPortal == alivePlayerCount;
            if (_isAllplayersinPortal.Value == isAllAlivePlayersInPortal)
                return;

            _isAllplayersinPortal.Value = isAllAlivePlayersInPortal;
            if (isAllAlivePlayersInPortal)
            {
                TimerController.SetPortalInAllPlayersCountRpc();
                return;
            }

            TimerController.SetNormalCountRpc();
        }

        protected override void AwakeInit()
        {
        }



        [Rpc(SendTo.ClientsAndHost)]
        public void EnteredPlayerInPortalRpc(ulong playerIndex)
        {
            if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(playerIndex,out NetworkObject player))
            {
                if (player.gameObject.TryGetComponentInChildren(out UIPortalIndicator indicator))
                {
                    indicator.SetIndicatorOn();
                }
            }
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void ExitedPlayerInPortalRpc(ulong playerIndex)
        {
            if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(playerIndex, out NetworkObject player))
            {
                if (player.gameObject.TryGetComponentInChildren(out UIPortalIndicator indicator))
                {
                    indicator.SetIndicatorOff();
                }
            }
        }
    }
}
