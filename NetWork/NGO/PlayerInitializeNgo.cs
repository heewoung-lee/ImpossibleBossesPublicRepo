using System.Collections;
using System.Collections.Generic;
using Controller;
using GameManagers.GameManagerExManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using Module.PlayerModule;
using NetWork.BaseNGO;
using Stats;
using UI.Scene.Interface;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO
{
    public class PlayerInitializeNgo : NetworkBehaviourBase, ISceneChangeBehaviour
    {
        private IResourcesServices _resourcesServices;
        private IPlayerSpawnManager _gameManagerEx;
        private RelayManager _relayManager;
        private Coroutine _pushBackCoroutine;

        public class CharacterSpawnFactory : NgoZenjectFactory<PlayerInitializeNgo>
        {
            public CharacterSpawnFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService,
                string key) : base(container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>(key);
            }
        }

        [Inject]
        public void Construct(
            IResourcesServices resourcesServices,
            IPlayerSpawnManager gameManagerEx,
            RelayManager relayManager)
        {
            _resourcesServices = resourcesServices;
            _gameManagerEx = gameManagerEx;
            _relayManager = relayManager;
        }

        enum Transforms
        {
            Interaction
        }

        private Transform _interactionTr;

        protected override void AwakeInit()
        {
        }

        public override void OnNetworkSpawn()
        {
            UtilDebug.Log($"OnNetworkSpawn{gameObject.name}");
            base.OnNetworkSpawn();
            if (IsOwner)
            {
                Bind<Transform>(typeof(Transforms));
                _interactionTr = Get<Transform>((int)Transforms.Interaction);
                SetOwnerPlayerADD_Module();
                _gameManagerEx.SetPlayer(gameObject);
            }

            _relayManager.NetworkManagerEx.SceneManager.OnLoadEventCompleted += SetParentPosition;
        }

        private void SetParentPosition(
            string sceneName,
            LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            if (_relayManager.NetworkManagerEx.IsHost == false)
                return;

            if (loadSceneMode != LoadSceneMode.Single)
                return;

            GetComponent<NetworkObject>().TrySetParent(_relayManager.NgoRoot.transform);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _relayManager.NetworkManagerEx.SceneManager.OnLoadEventCompleted -= SetParentPosition;
        }

        protected override void StartInit()
        {
        }

        public void SetOwnerPlayerADD_Module()
        {
            gameObject.name = "OnwerPlayer";
            gameObject.GetComponent<PlayerStats>().enabled = true;

            _resourcesServices.GetOrAddComponent<PlayerInput>(gameObject);
            PlayerController controller = _resourcesServices.GetOrAddComponent<PlayerController>(gameObject);

            _resourcesServices.GetOrAddComponent<ModulePlayerAnimInfo>(gameObject);
            _resourcesServices.GetOrAddComponent<PlayerSceneOpeningInputLockBehaviour>(gameObject);
            _resourcesServices.GetOrAddComponent<ModulePlayerTextureCamera>(gameObject);
            _resourcesServices.GetOrAddComponent<ModulePlayerInteraction>(_interactionTr.gameObject);
            SetPlayerLayerMask();

            _gameManagerEx.InvokePlayerSpawnWithController(controller);
        }

        private void SetPlayerLayerMask()
        {
            LayerMask playerMask = LayerMask.NameToLayer("Player");
            gameObject.layer = playerMask;
            foreach (Transform childtr in gameObject.transform)
            {
                if (childtr.TryGetComponent(out ModulePlayerLayerField moduleField))
                {
                    moduleField.gameObject.layer = playerMask;
                    SetPlayerLayerMask(moduleField.transform, playerMask);
                }
            }
        }

        private void SetPlayerLayerMask(Transform setLayerMaskTr, LayerMask layerMask)
        {
            foreach (Transform childtr in setLayerMaskTr.transform)
            {
                childtr.gameObject.layer = layerMask;
                SetPlayerLayerMask(childtr, layerMask);
            }
        }

        public void OnBeforeSceneUnload()
        {
            if (IsOwner == false)
                return;

            gameObject.transform.SetParent(null, false);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SetForcePositionFromNetworkRpc(Vector3 position)
        {
            if (TryGetComponent(out NavMeshAgent agent))
            {
                agent.ResetPath();
                if (agent.isOnNavMesh)
                {
                    agent.Warp(position);
                }
                else
                {
                    transform.position = position;
                }
            }
            else
            {
                transform.position = position;
            }

            if (IsOwner == false)
                return;

            if (TryGetComponent(out PlayerController controller))
            {
                controller.CurrentStateType = controller.BaseIDleState;
            }

            if (TryGetComponent(out NetworkTransform networkTransform))
            {
                networkTransform.Teleport(position, transform.rotation, transform.localScale);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void PushBackFromNetworkRpc(Vector3 dir, float distance, float duration)
        {
            if (IsOwner == false)
                return;

            if (distance <= 0f || duration <= 0f)
                return;

            if (_pushBackCoroutine != null)
            {
                StopCoroutine(_pushBackCoroutine);
            }

            _pushBackCoroutine = StartCoroutine(PushBackRoutine(dir.normalized, distance, duration));
        }

        private IEnumerator PushBackRoutine(Vector3 dir, float totalDistance, float duration)
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                yield break;
            }

            agent.ResetPath();

            if (TryGetComponent(out PlayerController controller))
            {
                controller.CurrentStateType = controller.BaseIDleState;
            }

            float elapsedTime = 0f;
            float speed = totalDistance / duration;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                agent.Move(dir * speed * Time.deltaTime);
                yield return null;
            }

            _pushBackCoroutine = null;
        }
    }
}