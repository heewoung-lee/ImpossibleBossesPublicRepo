using BehaviorDesigner.Runtime;
using Character.Skill.AllofSkills.BossMonster.DarkWizard;
using Controller;
using Enemy.Boss.Darkwizard.Minion;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using Stats.BossStats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Util;
using VFX;
using Zenject;

namespace Module.EnemyModule.Boss.DarkWizard
{
    public class NGOModuleDarkWizardAnimationEvent : NetworkBehaviour
    {
        [Header("Minion Spawn")]
        [SerializeField] private float _spawnMinRadius = 2.5f;
        [SerializeField] private float _spawnMaxRadius = 6f;
        [SerializeField] private float _edgePadding = 1.2f;
        [SerializeField] private int _maxSampleCount = 20;
        [SerializeField] private float _navMeshSampleDistance = 2f;
        
        
        private const string NGOAttackMuzzlePath = "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardAttackMuzzle";
        private const string NGOSpawnIndicatorPath = "Prefabs/Enemy/Boss/Indicator/NGOSpawnIndicator";
        
        
        private RelayManager _relayManager;
        private IResourcesServices _resourcesServices;
        private IVFXManagerServices _vfxManagerServices;
        
        
        private BossController _bossController;
        private DarkWizardSoundAnimationEvent _darkWizardSoundAnimationEvent;
        private GameObject _targetObj;
        private NetworkObjectReference _attackerNetworkRef;
        private DarkWizardAttackPosition _attackerPosition;


        [Inject]
        public void Construct(RelayManager relayManager,IResourcesServices resourceService,IVFXManagerServices vfxManagerServices)
        {
            _relayManager = relayManager;
            _resourcesServices = resourceService;
            _vfxManagerServices = vfxManagerServices;
        }


        private void Awake()
        {
            _bossController = GetComponent<BossController>();
            _darkWizardSoundAnimationEvent = GetComponent<DarkWizardSoundAnimationEvent>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _attackerNetworkRef = new NetworkObjectReference(NetworkObject);
            _attackerPosition = GetComponentInChildren<DarkWizardAttackPosition>();
        }


        public void DarkWizardAttack()
        {
            _darkWizardSoundAnimationEvent.DarkWizardAttackSfxEvent();

            if (IsHost == false) return;

            GameObject projectile = _resourcesServices.InstantiateByKey("Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardAttack");
            _relayManager.SpawnNetworkObj(projectile,position:_attackerPosition.transform.position);
            
            if (projectile.TryGetComponent(out DarkWizardHomingBullet bullet))
            {
                var target = _bossController.TargetObjectInBehaviourTree;
                bullet.FireRpc(_attackerNetworkRef,target, NetworkManager.ServerTime.Time);
                _vfxManagerServices.InstantiateParticleInArea(NGOAttackMuzzlePath,_attackerPosition.transform.position);
            }
            
        }

        public void DarkWizardSpawnIndicator()
        {
            _darkWizardSoundAnimationEvent.DarkWizardSpawnMinionSfxEvent();

            if (IsHost == false)
            {
                return;
            }

            if (TryGetRandomSpawnPosition(_bossController.transform.position, out Vector3 spawnPosition) == false)
            {
                return;
            }

            GameObject spawnedIndicator = _resourcesServices.InstantiateByKey(NGOSpawnIndicatorPath);
            spawnedIndicator = _relayManager.SpawnNetworkObj(spawnedIndicator,position: spawnPosition);
            if (spawnedIndicator.TryGetComponent(out NgoIndicatorController indicatorController))
            {
                indicatorController.SetSpawnerBossNetworkObjectId(NetworkObjectId);
            }
            //여기서는 인디케이터를 스폰만 하고,
            //인디케이터 로직은 인디게이터에서 작성
        }
        
        
        private bool TryGetRandomSpawnPosition(Vector3 center, out Vector3 spawnPosition)
        {
            for (int i = 0; i < _maxSampleCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(_spawnMinRadius, _spawnMaxRadius);
                Vector3 candidate = center + new Vector3(randomCircle.x, 0f, randomCircle.y);

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _navMeshSampleDistance, NavMesh.AllAreas) == false)
                {
                    continue;
                }

                if (NavMesh.FindClosestEdge(hit.position, out NavMeshHit edgeHit, NavMesh.AllAreas) == false)
                {
                    continue;
                }

                if (edgeHit.distance < _edgePadding)
                {
                    continue;
                }

                spawnPosition = hit.position;
                return true;
            }

            spawnPosition = center;
            return false;
        }
       
        
    }
}
