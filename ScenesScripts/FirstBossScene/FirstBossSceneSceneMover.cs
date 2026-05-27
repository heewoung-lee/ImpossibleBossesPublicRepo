using GameManagers.RelayManagement;
using GameManagers.SceneManagement;
using NetWork.NGO;

using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace ScenesScripts.BattleScene
{
    public class FirstBossSceneSceneMover : ISceneMover
    {
        private readonly SceneManagerEx _sceneManagerEx;
        private readonly RelayManager _relayManager;

        [Inject]
        public FirstBossSceneSceneMover(SceneManagerEx sceneManagerEx, RelayManager relayManager)
        {
            _sceneManagerEx = sceneManagerEx;
            _relayManager = relayManager;
        }
        

        public void MoveScene()
        {
            if (_relayManager.NetworkManagerEx.IsHost == false)
                return;


            _relayManager.NetworkManagerEx.NetworkConfig.EnableSceneManagement = true;
            _sceneManagerEx.OnAllPlayerLoadedEvent += SetPosition;
            _sceneManagerEx.NetworkLoadScene(Define.SceneName.FirstBossScene);
            _relayManager.NgoRPCCaller.ResetManagersRpc();


            void SetPosition()
            {
                foreach (NetworkObject spawnedObject in _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjectsList)
                {
                    if (spawnedObject.TryGetComponent(out PlayerInitializeNgo initializeNgo) == false)
                    {
                        continue;
                    }

                    Vector3 pos = new Vector3(spawnedObject.OwnerClientId, 0, 0);
                    initializeNgo.SetForcePositionFromNetworkRpc(pos);
                }
                _sceneManagerEx.OnAllPlayerLoadedEvent -= SetPosition;
            }
        }
    }
}
