using System;
using System.Collections.Generic;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using NetWork.NGO;
using ScenesScripts.FirstBossScene.Installer;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;
using ZenjectContext.ProjectContextInstaller;

namespace ScenesScripts.FirstBossScene
{
    public class MockSceneLoadingSync : NetworkBehaviour
    {
        public class MockSceneLoadingSyncFactory : NgoZenjectFactory<MockSceneLoadingSync>, IMockSceneLoadingSyncFactory
        {
            [Inject]
            public MockSceneLoadingSyncFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/MockSceneLoadingSync");
            }

            public MockSceneLoadingSync Create(int expectedPlayerCount)
            {
                MockSceneLoadingSync loadingSync = base.Create(null);
                loadingSync.Initialize(expectedPlayerCount);
                return loadingSync;
            }
        }

        private RelayManager _relayManager;
        private SignalBus _signalBus;
        private readonly HashSet<ulong> _readyClients = new HashSet<ulong>();
        private Action<RpcCallerReadySignal> _onRpcCallerReady;
        private int _expectedPlayerCount = 1;
        private bool _hasReportedLocalReady;
        private bool _hasInitializedServerState;

        [Inject]
        public void Construct(RelayManager relayManager, SignalBus signalBus)
        {
            _relayManager = relayManager;
            _signalBus = signalBus;
        }

        public void Initialize(int expectedPlayerCount)
        {
            _expectedPlayerCount = Mathf.Max(1, expectedPlayerCount);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                ResetServerState();
                _hasInitializedServerState = true;
            }

            RegisterLocalReadyTrigger();
        }

        private void RegisterLocalReadyTrigger()
        {
            if (_relayManager.NgoRPCCaller != null)
            {
                ReportLocalReady();
                return;
            }

            _onRpcCallerReady = signal =>
            {
                _signalBus.Unsubscribe<RpcCallerReadySignal>(_onRpcCallerReady);
                _onRpcCallerReady = null;
                ReportLocalReady();
            };
            _signalBus.Subscribe<RpcCallerReadySignal>(_onRpcCallerReady);
        }

        private void ReportLocalReady()
        {
            if (_hasReportedLocalReady)
            {
                return;
            }

            _hasReportedLocalReady = true;

            if (IsServer)
            {
                ProcessReadyClient(_relayManager.NetworkManagerEx.LocalClientId);
                return;
            }

            ReportLoadingReadyServerRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ReportLoadingReadyServerRpc(RpcParams rpcParams = default)
        {
            ProcessReadyClient(rpcParams.Receive.SenderClientId);
        }

        private void ProcessReadyClient(ulong clientId)
        {
            if (_hasInitializedServerState == false)
            {
                ResetServerState();
                _hasInitializedServerState = true;
            }

            if (_readyClients.Add(clientId) == false)
            {
                return;
            }

            if (_readyClients.Count < _expectedPlayerCount)
            {
                return;
            }

            _relayManager.NgoRPCCaller.LoadedPlayerCount = _expectedPlayerCount;
            _relayManager.NgoRPCCaller.IsAllPlayerLoaded = true;
        }

        private void ResetServerState()
        {
            _readyClients.Clear();
            if (_relayManager.NgoRPCCaller == null)
            {
                return;
            }

            _relayManager.NgoRPCCaller.LoadedPlayerCount = 0;
            _relayManager.NgoRPCCaller.IsAllPlayerLoaded = false;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (_onRpcCallerReady != null)
            {
                _signalBus.Unsubscribe<RpcCallerReadySignal>(_onRpcCallerReady);
                _onRpcCallerReady = null;
            }

            _hasReportedLocalReady = false;

            if (IsServer)
            {
                _readyClients.Clear();
                _hasInitializedServerState = false;
            }
        }
    }
}
