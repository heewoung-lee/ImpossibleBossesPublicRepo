using System;
using System.Collections.Generic;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ResourcesManager;
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
        
        public class CharacterSpawnFactory : NgoZenjectFactory<string,PlayerInitializeNgo>
        {
            public CharacterSpawnFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService,
                string key) : base(container, registerableFactory, handlerFactory, loadService, key)
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
            Debug.Log($"OnNetworkSpawn{gameObject.name}");
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

            transform.SetParent(_relayManager.NgoRoot.transform);
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

            _resourcesServices.GetOrAddComponent<ModuleMainCameraCinemachineBrain>(gameObject);
            _resourcesServices.GetOrAddComponent<ModulePlayerAnimInfo>(gameObject);
            _resourcesServices.GetOrAddComponent<ModulePlayerTextureCamera>(gameObject);
            _resourcesServices.GetOrAddComponent<ModuleMainCameraCinemachineBrain>(gameObject);

            _resourcesServices.GetOrAddComponent(GetPlayerModuleClass(_relayManager.ChoicePlayerCharacter),gameObject);
            
            _resourcesServices.GetOrAddComponent<ModulePlayerInteraction>(_interactionTr.gameObject);
            SetPlayerLayerMask();
      
            RuntimeAnimatorController ownerPlayerAnimController = _resourcesServices.Load<RuntimeAnimatorController>($"Art/Player/AnimData/Animation/{_relayManager.ChoicePlayerCharacter}Controller");
            gameObject.GetComponent<Animator>().runtimeAnimatorController = ownerPlayerAnimController;
            _gameManagerEx.InvokePlayerSpawnWithController(controller);
        }


        private Type GetPlayerModuleClass(Define.PlayerClass playerClass)
        {
            switch (playerClass)
            {
                case Define.PlayerClass.Archer:
                    return typeof(ModuleAcherClass); 
                case Define.PlayerClass.Fighter:
                    return typeof(ModuleFighterClass);
                case Define.PlayerClass.Mage:
                    return typeof(ModuleMageClass);
                case Define.PlayerClass.Monk:
                    return typeof(ModuleMonkClass);
                case Define.PlayerClass.Necromancer:
                    return typeof(ModuleNecromancerClass);

                default: return null;
            }
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
