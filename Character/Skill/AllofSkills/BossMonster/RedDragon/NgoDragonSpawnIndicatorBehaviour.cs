using GameManagers.RelayManagement;
using Unity.Netcode;
using UnityEngine;
using VFX;
using Zenject;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    [RequireComponent(typeof(NgoIndicatorController))]
    public class NgoDragonSpawnIndicatorBehaviour : NetworkBehaviour
    {
        private const string RootBinderPath = "Prefabs/Enemy/Minion/RootBinder";

        private RelayManager _relayManager;
        private NgoIndicatorController _indicatorController;
        private bool _hasSpawnedRootBinder;

        [Inject]
        public void Construct(RelayManager relayManager)
        {
            _relayManager = relayManager;
        }

        private void Awake()
        {
            _indicatorController = GetComponent<NgoIndicatorController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _hasSpawnedRootBinder = false;

            if (_indicatorController != null)
            {
                _indicatorController.OnIndicatorDone += SpawnRootBinder;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (_indicatorController != null)
            {
                _indicatorController.OnIndicatorDone -= SpawnRootBinder;
            }

            _hasSpawnedRootBinder = false;
        }

        private void SpawnRootBinder()
        {
            if (IsHost == false || _hasSpawnedRootBinder)
            {
                return;
            }

            _hasSpawnedRootBinder = true;

            Vector3 spawnPosition = _indicatorController != null
                ? _indicatorController.Position
                : transform.position;

            _relayManager.SpawnNetworkObj(RootBinderPath, position: spawnPosition);
        }
    }
}
