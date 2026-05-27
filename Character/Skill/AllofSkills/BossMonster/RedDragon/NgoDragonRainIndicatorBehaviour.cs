using GameManagers.ResourcesExManagement;
using GameManagers.RelayManagement;
using Unity.Netcode;
using UnityEngine;
using VFX;
using Zenject;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    [RequireComponent(typeof(NgoIndicatorController))]
    public class NgoDragonRainIndicatorBehaviour : NetworkBehaviour
    {
        private const string DragonRainDropPath = "Prefabs/Enemy/Boss/AttackPattern/RedDragon/DragonRainDropVFX";
        private const float RainDropLifetime = 2f;

        private RelayManager _relayManager;
        private IResourcesServices _resourcesServices;
        private NgoIndicatorController _indicatorController;
        private bool _hasSpawnedRainDrop;
        [SerializeField, Range(0f, 1f)] private float _rainDropSpawnProgressThreshold = 0.7f;

        [Inject]
        public void Construct(RelayManager relayManager, IResourcesServices resourcesServices)
        {
            _relayManager = relayManager;
            _resourcesServices = resourcesServices;
        }

        private void Awake()
        {
            _indicatorController = GetComponent<NgoIndicatorController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _hasSpawnedRainDrop = false;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _hasSpawnedRainDrop = false;
        }

        private void Update()
        {
            if (IsHost == false || _hasSpawnedRainDrop || _indicatorController == null)
            {
                return;
            }

            if (_indicatorController.NormalizedProgress < _rainDropSpawnProgressThreshold)
            {
                return;
            }

            _hasSpawnedRainDrop = true;

            GameObject rainDropObject = _relayManager.SpawnNetworkObj(
                DragonRainDropPath,
                position: _indicatorController.Position);

            if (rainDropObject == null)
            {
                return;
            }

            _resourcesServices.DestroyObject(rainDropObject, RainDropLifetime);
        }
    }
}
