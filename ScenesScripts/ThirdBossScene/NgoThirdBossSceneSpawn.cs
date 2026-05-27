using System.Collections.Generic;
using Controller.BossState.BossRedDragon;
using GameManagers.GameManagerExManagement;
using GameManagers.NGOPoolManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using NetWork.NGO.Scene_NGO;
using ScenesScripts.CommonInstaller;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;
using ZenjectContext.ProjectContextInstaller;

namespace ScenesScripts.ThirdBossScene
{
    public class NgoThirdBossSceneSpawn : NetworkBehaviourBase
    {
        
        public class NgoThirdBossSceneSpawnFactory : NgoZenjectFactory<NgoThirdBossSceneSpawn>
        {
            public NgoThirdBossSceneSpawnFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NgoThirdBossSceneSpawn");
            }
        }
        private RelayManager _relayManager;
        private NgoPoolManager _poolManager;
        private IResourcesServices _resourcesServices;
        private IPlayerSpawnManager _playerSpawnManager;
        private SpawnPosition _spawnPosition;
        private SignalBus _signalBus;
        private BossSceneOpeningCinematicCoordinator _bossSceneOpeningCinematicCoordinator;
        private readonly HashSet<ulong> _openingReadyClientIds = new HashSet<ulong>();
        private bool _hasReportedLocalOpeningReady;
        private const double OpeningStartLeadTime = 0.25d;


        [Inject]
        public void Construct(RelayManager relayManager, NgoPoolManager poolManager,
            IResourcesServices resourcesServices, IPlayerSpawnManager playerSpawnManager, SpawnPosition spawnPosition,
            SignalBus signalBus)
        {
            _relayManager = relayManager;
            _poolManager = poolManager;
            _resourcesServices = resourcesServices;
            _playerSpawnManager = playerSpawnManager;
            _spawnPosition = spawnPosition;
            _signalBus = signalBus;
            _signalBus.Subscribe<BossSceneOpeningLocalReadySignal>(HandleLocalOpeningReady);
        }


        GameObject _player;

        protected override void AwakeInit()
        {
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            HostSpawnObject();
            _poolManager.Create_NGO_Pooling_Object();
            TryConsumeReportedOpeningReady();
        }

        public override void OnNetworkDespawn()
        {
            _signalBus.Unsubscribe<BossSceneOpeningLocalReadySignal>(HandleLocalOpeningReady);
            _openingReadyClientIds.Clear();
            _hasReportedLocalOpeningReady = false;
        }

        private void HostSpawnObject()
        {
            if (IsHost == false)
                return;

            BossRedDragonController bossRedDragonController = _resourcesServices
                .InstantiateByKey("Prefabs/Enemy/Boss/Character/RedDragon").GetComponent<BossRedDragonController>();
            GameObject spawnedBoss = _relayManager.SpawnNetworkObj(
                bossRedDragonController.gameObject,
                _relayManager.NgoRoot.transform,
                _spawnPosition.BossSpawnPosition);
            FaceBossToHostPlayer(spawnedBoss.transform);
            
            
        }

        private void FaceBossToHostPlayer(Transform bossTransform)
        {
            GameObject player = _playerSpawnManager.GetPlayer();
            if (player == null)
            {
                return;
            }

            Vector3 lookDirection = player.transform.position - bossTransform.position;
            lookDirection.y = 0f;
            bossTransform.rotation = Quaternion.LookRotation(lookDirection);
        }

        protected override void StartInit()
        {
        }

        private void TryConsumeReportedOpeningReady()
        {
            if (_hasReportedLocalOpeningReady)
            {
                return;
            }

            _bossSceneOpeningCinematicCoordinator ??=
                GameObject.FindAnyObjectByType<BossSceneOpeningCinematicCoordinator>();

            if (_bossSceneOpeningCinematicCoordinator == null ||
                _bossSceneOpeningCinematicCoordinator.HasReportedOpeningReady == false)
            {
                return;
            }

            HandleLocalOpeningReady();
        }

        private void HandleLocalOpeningReady()
        {
            if (_hasReportedLocalOpeningReady || IsSpawned == false)
            {
                return;
            }

            _hasReportedLocalOpeningReady = true;

            if (IsHost)
            {
                RegisterOpeningReadyClient(_relayManager.NetworkManagerEx.LocalClientId);
                return;
            }

            ReportOpeningReadyServerRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ReportOpeningReadyServerRpc(RpcParams rpcParams = default)
        {
            RegisterOpeningReadyClient(rpcParams.Receive.SenderClientId);
        }

        private void RegisterOpeningReadyClient(ulong clientId)
        {
            if (IsHost == false || _openingReadyClientIds.Add(clientId) == false)
            {
                return;
            }

            if (_openingReadyClientIds.Count < _relayManager.CurrentUserCount)
            {
                return;
            }

            double startServerTime = _relayManager.NetworkManagerEx.ServerTime.Time + OpeningStartLeadTime;
            BroadcastOpeningStartRpc(startServerTime);
            _openingReadyClientIds.Clear();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void BroadcastOpeningStartRpc(double startServerTime)
        {
            _signalBus.Fire(new BossSceneOpeningStartSignal
            {
                StartServerTime = startServerTime
            });
        }
    }
}
