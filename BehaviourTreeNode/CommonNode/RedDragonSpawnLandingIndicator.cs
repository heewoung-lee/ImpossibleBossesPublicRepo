using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using Unity.Netcode;
using UnityEngine;
using Util;
using VFX;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonSpawnLandingIndicator : Action
    {
        private const string IndicatorPath = "Prefabs/Enemy/Boss/Indicator/NGOLandingIndicator";

        [SerializeField] private SharedProjector _landingIndicator;
        [SerializeField] private float _radius = 8f;
        [SerializeField] private float _arc = 360f;
        [SerializeField] private float _duration = 1.5f;

        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        private bool _hasSpawned;

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

        public override void OnStart()
        {
            base.OnStart();
            _hasSpawned = false;
            SpawnLandingIndicator();
        }

        public override TaskStatus OnUpdate()
        {
            return _hasSpawned ? TaskStatus.Success : TaskStatus.Failure;
        }

        private void SpawnLandingIndicator()
        {
            GameObject indicatorObject = ResourcesServices.InstantiateByKey(IndicatorPath);
            NgoIndicatorController indicatorController = indicatorObject.GetComponent<NgoIndicatorController>();
            if (indicatorController == null)
            {
                UtilDebug.LogError($"[{nameof(RedDragonSpawnLandingIndicator)}] {nameof(NgoIndicatorController)} is missing.");
                return;
            }

            Vector3 spawnPosition = GetIndicatorPosition();
            _landingIndicator.Value = indicatorController;
            indicatorController = RelayManager.SpawnNetworkObj(indicatorController.gameObject)
                .GetComponent<NgoIndicatorController>();
            if (Owner.TryGetComponent(out NetworkObject ownerNetworkObject))
            {
                indicatorController.SetSpawnerBossNetworkObjectId(ownerNetworkObject.NetworkObjectId);
            }
            indicatorController.SetValue(_radius, _arc, spawnPosition, _duration);
            _landingIndicator.Value = indicatorController;
            _hasSpawned = true;
        }

        private Vector3 GetIndicatorPosition()
        {
            Vector3 ownerPosition = Owner.transform.position;
            return new Vector3(ownerPosition.x, 0f, ownerPosition.z);
        }
    }
}
