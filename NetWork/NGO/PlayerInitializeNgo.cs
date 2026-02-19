using System;
using System.Collections.Generic;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ResourcesManager;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using Module.CameraModule;
using Module.PlayerModule;
using Module.PlayerModule.PlayerClassModule;
using NetWork.BaseNGO;
using Player;
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
        
        public class CharacterSpawnFactory : NgoZenjectFactory<PlayerInitializeNgo>
        {
            public CharacterSpawnFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService,
                string key) : base(container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>(key);
            }
        }
        
        [Inject] 
        public void Construct(IResourcesServices resourcesServices ,IPlayerSpawnManager gameManagerEx,RelayManager relayManager)
        {
            _resourcesServices = resourcesServices;
            _gameManagerEx = gameManagerEx;
            _relayManager = relayManager;
        }
        

        enum Transforms
        {
            Interaction
        }
        Transform _interactionTr;
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
                _gameManagerEx.SetPlayer(gameObject);
                SetOwnerPlayerADD_Module();
            }
            _relayManager.NetworkManagerEx.SceneManager.OnLoadEventCompleted += SetParentPosition;
            
            
        }
        private void SetParentPosition(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
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
            _resourcesServices.GetOrAddComponent<ModulePlayerTextureCamera>(gameObject);
            //플레이어 렌더 카메라는 붙이는걸로 진행따로 카메라 바인드 안할예정

            _resourcesServices.GetOrAddComponent<ModulePlayerInteraction>(_interactionTr.gameObject);
            SetPlayerLayerMask();
      
            //RuntimeAnimatorController ownerPlayerAnimController = _resourcesServices.Load<RuntimeAnimatorController>($"Art/Player/AnimData/Animation/{_relayManager.ChoicePlayerCharacter}Controller");
            //gameObject.GetComponent<Animator>().runtimeAnimatorController = ownerPlayerAnimController;
            //1.6일 삭제 각 클래스별 필수적으로 가져야 할 자신의 애니메이션 컨트롤러를 고정적으로 넣어둠 
            //앞으로는 로드해서 집어넣을 필요가 없음
            
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
        private void SetPlayerLayerMask(Transform setLayerMaskTr,LayerMask layerMask)
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
            if (IsOwner == false)
                return;

            GetComponent<NavMeshAgent>().ResetPath();//플레이어가 이동중이라면 경로를 없앤다
            PlayerController controller = GetComponent<PlayerController>();// 상태를 IDLE로 강제로 바꾼다
            controller.CurrentStateType = controller.BaseIDleState;
            GetComponent<NetworkTransform>().Teleport(position,transform.rotation,transform.localScale);
            //포지션을 호스트가 바꾸는데 NavMesh에 대한 포지션만 변경하므로 NEtwork에는 업데이트가 안될 수 도 있기에
            //각자가 네트워크에서 포지션을 업데이트 해준다. 캐싱은 필요없음 씬전환시에만 호출 해서 쓸거기 때문에 캐싱은 안함
        }
    }
}
