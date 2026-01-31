using System;
using System.Collections.Generic;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using NUnit.Framework;
using Scene.CommonInstaller;
using UI.Scene.Interface;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO
{
    public interface ISpawnController
    {
        public void SpawnControllerOption(NetworkObject ngo,Action spawnLogic);
    }

    public class NgoRPCSpawnController : NetworkBehaviour, ISceneChangeBehaviour, ISpawnController, IDisposable
    {
        private RelayManager _relayManager;
        private HashSet<ulong> _spawnedClients;
        private IRegistrar<ISpawnController> _ngoRPCSpawnController;

        [Inject]
        public void Construct(RelayManager relayManager, IRegistrar<ISpawnController> ngoRPCSpawnController)
        {
            _relayManager = relayManager;
            _ngoRPCSpawnController = ngoRPCSpawnController;
        }

        public class NgoRPCSpawnerFactory : NgoZenjectFactory<NgoRPCSpawnController>
        {
            public NgoRPCSpawnerFactory(DiContainer container, IFactoryManager factoryManager, NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(container, factoryManager, handlerFactory, loadService)
            {
            _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NgoRPCSpawnController");
            }
        }

        public void OnBeforeSceneUnload()
        {
            Debug.Log("리스트 초기화 완료");
            _spawnedClients.Clear();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _spawnedClients = new HashSet<ulong>();
            _ngoRPCSpawnController.Register(this);

            if (IsServer)
            {
                _relayManager.NetworkManagerEx.CustomMessagingManager.RegisterNamedMessageHandler("ClientReady",
                    SendClientReady);
            }

            void SendClientReady(ulong sender,FastBufferReader reader)
            {
                Debug.Log($"{sender}번쨰 클라이언트가 신호 보냄");
                ClientToReady(sender);
            }
        }

        public void ClientToReady(ulong senderId)
        {
            Debug.Log($"준비신호를 보낸 클라이언트 : {senderId}");
            _spawnedClients.Add(senderId);
            foreach (ulong variable in _spawnedClients)
            {
                Debug.Log($"현재 {variable}번째 클라이언트가 준비됨.");
            }
            // 서버가 직접 전체 오브젝트 훑고, sender에게 안 보이는 것만 Show
            foreach (NetworkObject ngo in _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjectsList)
            {
                if (ngo == null || ngo.IsSpawned == false) continue;

                //만약 ngo가 sender에게 안보인다면
                if (ngo.IsNetworkVisibleTo(senderId) == false)
                {
                    ngo.NetworkShow(senderId);
                    //보이도록 호출
                }
            }
        }

        public void ShowNgoForReadyClient(ulong ngoID)
        {
            _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(ngoID, out NetworkObject ngo);
            Assert.IsNotNull(ngo,"ngo is null");
            foreach (ulong readyClientID in _spawnedClients)
            {
                if (ngo.IsNetworkVisibleTo(readyClientID) == false)
                {
                    ngo.NetworkShow(readyClientID);
                    //보이도록 호출
                }
            }
        }

        public void SpawnControllerOption(NetworkObject ngo,Action spawnLogic)
        {
            if (ngo.SpawnWithObservers == true)
            {
                ngo.SpawnWithObservers = false;
            }
            spawnLogic.Invoke();
            ShowNgoForReadyClient(ngo.NetworkObjectId);
        }

        public void Dispose()
        {
            _ngoRPCSpawnController.Unregister(this);
        }
    }
}