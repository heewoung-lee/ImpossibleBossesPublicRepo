using GameManagers;
using GameManagers.RelayManager;
using GameManagers.Scene;
using NetWork.NGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Util;
using Zenject;

namespace Scene.BattleScene
{
    public class BattleSceneMover : ISceneMover
    {
        private readonly SceneManagerEx _sceneManagerEx;
        private readonly RelayManager _relayManager;

        [Inject]
        public BattleSceneMover(SceneManagerEx sceneManagerEx, RelayManager relayManager)
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
            _sceneManagerEx.NetworkLoadScene(Define.Scene.BattleScene);
            _relayManager.NgoRPCCaller.ResetManagersRpc();


            void SetPosition()
            {
                foreach (NetworkObject player in _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjectsList)
                {
                    Vector3 pos = new Vector3(player.OwnerClientId, 0, 0);

                    if (player.TryGetComponent(out NavMeshAgent agent))
                    {
                        agent.ResetPath();
                        agent.Warp(pos);
                        if (player.TryGetComponent(out PlayerInitializeNgo initializeNgo))
                        {
                            initializeNgo.SetForcePositionFromNetworkRpc(pos);
                        }
                    }
                }
                _sceneManagerEx.OnAllPlayerLoadedEvent -= SetPosition;
            }
        }
    }
}
