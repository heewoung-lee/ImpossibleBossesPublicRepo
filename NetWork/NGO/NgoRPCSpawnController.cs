using System;
using System.Collections.Generic;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using Scene.CommonInstaller;
using UI.Scene.Interface;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO
{
    public interface ISpawnController
    {
        public void SpawnControllerOption(NetworkObject ngo, Action spawnLogic);
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
            public NgoRPCSpawnerFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NgoRPCSpawnController");
            }
        }

        public void OnBeforeSceneUnload()
        {
            UtilDebug.Log("리스트 초기화 완료");
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

            void SendClientReady(ulong sender, FastBufferReader reader)
            {
                UtilDebug.Log($"{sender}번쨰 클라이언트가 신호 보냄");
                ClientToReady(sender);
            }
        }

        public void ClientToReady(ulong senderId)
        {
            UtilDebug.Log($"준비신호를 보낸 클라이언트 : {senderId}");
            _spawnedClients.Add(senderId);
            foreach (ulong variable in _spawnedClients)
            {
                UtilDebug.Log($"현재 {variable}번째 클라이언트가 준비됨.");
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


        public void ShowNgoForReadyClient(NetworkObject ngo)
        {
            if (ngo == null || ngo.IsSpawned == false)
            {
                UtilDebug.LogWarning($"ShowNgoForReadyClient Failed: NGO is null or not spawned.");
                return;
            }

            foreach (ulong readyClientID in _spawnedClients)
            {
                if (ngo.IsNetworkVisibleTo(readyClientID) == false)
                {
                    ngo.NetworkShow(readyClientID);
                    //보이도록 호출
                }
            }
        }

        public void SpawnControllerOption(NetworkObject ngo, Action spawnLogic)
        {
            if (ngo.SpawnWithObservers == true)
            {
                ngo.SpawnWithObservers = false;
            }
        
            spawnLogic.Invoke();
            ShowNgoForReadyClient(ngo);
        }
        


        public void Dispose()
        {
            _ngoRPCSpawnController.Unregister(this);
        }
        
        
        #region  LacacyCode 2.12일 네트워크 스폰쪽에서 문제생김 AI가 주인님 이게 더 좋아요라고 했다가 버그 생겼는데 원인을 몰라 한참을 찾음 결국 원인은 얘였음
        
        // public void SpawnControllerOption(NetworkObject ngo, Action spawnLogic)
        // {
        //     //CheckObjectVisibility는 서버에서만 실행됨.
        //     ngo.CheckObjectVisibility = (clientId) =>
        //     {
        //         // 서버(호스트)는 무조건 봐야 하므로 true
        //         if (clientId == _relayManager.NetworkManagerEx.LocalClientId) 
        //             return true;
        //
        //         // 그 외에는 준비된 클라이언트 목록에 있어야만 보임
        //         return _spawnedClients.Contains(clientId);
        //     };
        //     spawnLogic.Invoke();
        //     //ShowNgoForReadyClient(ngo);
        // }

        #endregion

    }
}