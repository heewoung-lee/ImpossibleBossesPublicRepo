using BehaviorDesigner.Runtime.Tasks;
using Controller;
using Controller.BossState;
using Controller.BossState.BossGolem;
using Controller.ControllerStats;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using System.Collections.Generic;
using NetWork;
using NetWork.BossGolem_NGO;
using Unity.Netcode;
using UnityEngine;
using Util;
using VFX;

namespace BehaviourTreeNode.BossGolem.Task
{
    [TaskCategory("CustomNode")]
    public class BossDead : Action
    {
        
        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;

        private RelayManager RelayManager
        {
            get
            {
                if (_relayManager == null)
                {
                    _relayManager = GetComponent<BossDependencyHub>().RelayManager;
                }
                return _relayManager;
            }
        }

        private IResourcesServices ResourcesServices
        {
            get
            {
                if (_resourcesServices == null)
                {
                    _resourcesServices = GetComponent<BossDependencyHub>().ResourcesServices;
                }
                return _resourcesServices;
            }
        }
        
        BossController _controller;
        private NGOBossNetworkController _networkController;
        private NetworkObject _ownerNetworkObject;
        private Animator _anim;
        private float _animLength;
        private bool _hasClearedOwnedIndicators;


        public override void OnAwake()
        {
            base.OnAwake();
            _controller = Owner.GetComponent<BossController>();
            _networkController = Owner.GetComponent<NGOBossNetworkController>();
            _ownerNetworkObject = Owner.GetComponent<NetworkObject>();
            _anim = Owner.GetComponent<Animator>();
            _animLength = Utill.GetAnimationLength("Anim_Death", _controller.Anim);
        }

        public override void OnStart()
        {
            base.OnStart();
            _controller.CurrentStateType = _controller.BaseDieState;
            _hasClearedOwnedIndicators = false;
            NetworkAnimationInfo animInfo = new NetworkAnimationInfo(_animLength, 0f, 0f, 0f, RelayManager.NetworkManagerEx.ServerTime.Time);
            _networkController.StartAnimChangedRpc(animInfo);
        }


        public override TaskStatus OnUpdate()
        {
            if (_controller.CurrentStateType == _controller.BaseDieState)
            {
                if (_hasClearedOwnedIndicators == false)
                {
                    ClearOwnedIndicators();
                    _hasClearedOwnedIndicators = true;
                }
                AnimatorStateInfo info = _anim.GetCurrentAnimatorStateInfo(0);
                bool isFinished = info.normalizedTime >= 1f && _anim.IsInTransition(0) == false;
                if (isFinished)
                {
                    return TaskStatus.Success;
                }
                return TaskStatus.Running;
            }
            return TaskStatus.Failure;

        }

        private void ClearOwnedIndicators()
        {
            if (_ownerNetworkObject == null || RelayManager.NetworkManagerEx.IsHost == false)
            {
                return;
            }

            ulong ownerNetworkObjectId = _ownerNetworkObject.NetworkObjectId;
            DestroyOwnedNgoIndicators(ownerNetworkObjectId);
        }

        private void DestroyOwnedNgoIndicators(ulong ownerNetworkObjectId)
        {
            List<NetworkObject> spawnedObjects = new List<NetworkObject>(RelayManager.NetworkManagerEx.SpawnManager.SpawnedObjectsList);
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                NetworkObject spawnedObject = spawnedObjects[i];
                if (spawnedObject == null || spawnedObject.IsSpawned == false)
                {
                    continue;
                }

                if (spawnedObject.TryGetComponent(out NgoIndicatorController circleIndicator) &&
                    circleIndicator.HasValidSpawnerBossNetworkObjectId &&
                    circleIndicator.SpawnerBossNetworkObjectId == ownerNetworkObjectId)
                {
                    ResourcesServices.DestroyObject(circleIndicator.gameObject);
                    continue;
                }

                if (spawnedObject.TryGetComponent(out NgoArrowIndicatorController arrowIndicator) &&
                    arrowIndicator.HasValidSpawnerBossNetworkObjectId &&
                    arrowIndicator.SpawnerBossNetworkObjectId == ownerNetworkObjectId)
                {
                    ResourcesServices.DestroyObject(arrowIndicator.gameObject);
                }
            }
        }
    }
}
