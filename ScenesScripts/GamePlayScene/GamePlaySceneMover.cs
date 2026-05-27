using GameManagers.RelayManagement;
using GameManagers.SceneManagement;
using NetWork.NGO;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace ScenesScripts.GamePlayScene
{
    public class GamePlaySceneMover : ISceneMover
    {
        private readonly BaseScene _baseScene;
        private readonly SceneManagerEx _sceneManagerEx;
        private readonly RelayManager _relayManager;

        [Inject]
        public GamePlaySceneMover(BaseScene baseScene, SceneManagerEx sceneManagerEx, RelayManager relayManager)
        {
            _baseScene = baseScene;
            _sceneManagerEx = sceneManagerEx;
            _relayManager = relayManager;
        }


        public void MoveScene()
        {
            if (_relayManager.NetworkManagerEx.IsHost == false)
                return;

            Define.SceneName nextScene = _sceneManagerEx.GetNextSceneByFlow(_baseScene.CurrentSceneName);

            _relayManager.NetworkManagerEx.NetworkConfig.EnableSceneManagement = true;
            _sceneManagerEx.OnClientLoadedEvent += SetLoadedClientPosition;
            _sceneManagerEx.NetworkLoadScene(nextScene);
            _relayManager.NgoRPCCaller.ResetManagersRpc();

            void SetLoadedClientPosition(ulong clientId)
            {
                BaseScene currentScene = _sceneManagerEx.GetCurrentScene;
                Vector3 targetPosition = currentScene is IHasSpawnPosition sceneWithSpawnPosition
                    ? sceneWithSpawnPosition.SpawnPosition.PlayerSpawnPosition + new Vector3(clientId, 0f, 0f)
                    : new Vector3(clientId, 0f, 0f);

                foreach (NetworkObject spawnedObject in _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjectsList)
                {
                    if (spawnedObject.OwnerClientId != clientId)
                    {
                        continue;
                    }

                    if (spawnedObject.TryGetComponent(out PlayerInitializeNgo initializeNgo) == false)
                    {
                        continue;
                    }

                    initializeNgo.SetForcePositionFromNetworkRpc(targetPosition);
                    break;
                }
            }
        }
    }
}
