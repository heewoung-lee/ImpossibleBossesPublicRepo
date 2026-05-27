using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using Unity.Netcode;
using UnityEngine;
using VFX;
using Zenject;
using Random = UnityEngine.Random;

namespace Enemy.Boss.Darkwizard.Minion
{
    public class NgoDarkWizardSpawnIndicatorBehaviour : NetworkBehaviour
    {
        private NgoIndicatorController _controller;
        private RelayManager _relayManager;
        private IResourcesServices _resourcesServices;
        private IVFXManagerServices _vfxManager;
        
        private const string NGOBomberPath = "Prefabs/Enemy/Minion/Bomber";
        private const string NGOMinionGolemPath = "Prefabs/Enemy/Minion/MinionGolem";
        private const string MinionSpawnImpactPath = "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/MinionSpawnImpact";
        


        [Inject]
        public void Construct(IResourcesServices resourcesServices, RelayManager relayManager, IVFXManagerServices vfxManager)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
            _vfxManager = vfxManager;
        }
        public void Awake()
        {
            _controller = GetComponent<NgoIndicatorController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _controller.SetValue(1f, 360f, transform.position, 1f,()=>SpawnMinion(gameObject,transform.position));
            
        }
        
        public void SpawnMinion(GameObject indicator, Vector3 spawnPosition)
        {
            if (IsHost == false)
            {
                return;
            }
            string spawnPath = Random.Range(0, 2) == 0 ? NGOBomberPath : NGOMinionGolemPath;
            _vfxManager.InstantiateParticleInArea(MinionSpawnImpactPath,transform.position,1f);
            _relayManager.SpawnNetworkObj(spawnPath, position: spawnPosition);
            _resourcesServices.DestroyObject(indicator);
        }

        
        
    }
}
