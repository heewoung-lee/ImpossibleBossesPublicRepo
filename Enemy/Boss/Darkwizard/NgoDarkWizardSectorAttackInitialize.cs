using GameManagers.ResourcesExManagement;
using GameManagers.RelayManagement;
using NetWork;
using NetWork.NGO;
using NetWork.BaseNGO;
using Stats.BossStats;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss.Darkwizard
{
    public class NgoDarkWizardSectorAttackInitialize : NgoPoolingInitializeBase
    {
        private NgoDarkWizardSectorAttackStraightMovement _straightMovement;
        private NgoDarkWizardSectorAttackCollisionBehaviour _collisionBehaviour;
        private RelayManager _relayManager;

        public class DarkWizardSectorAttackFactory : NgoZenjectFactory<NgoDarkWizardSectorAttackInitialize>
        {
            public DarkWizardSectorAttackFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardSectorAttack");
            }
        }

        [Inject]
        public void Construct(RelayManager relayManager)
        {
            _relayManager = relayManager;
        }

        private void Awake()
        {
            _straightMovement = GetComponent<NgoDarkWizardSectorAttackStraightMovement>();
            _collisionBehaviour = GetComponent<NgoDarkWizardSectorAttackCollisionBehaviour>();
        }

        public override void StartParticleOption(float duration, NetworkParams networkParams)
        {
            base.StartParticleOption(duration, networkParams);

            Vector3 fireDirection = networkParams.ArgPosVector3;
            fireDirection.y = 0f;

            if (fireDirection.sqrMagnitude <= 0.0001f)
            {
                fireDirection = Vector3.forward;
            }
            else
            {
                fireDirection.Normalize();
            }

            Vector3 currentPosition = transform.position;
            currentPosition.y = 1f;
            transform.SetPositionAndRotation(currentPosition, Quaternion.LookRotation(fireDirection, Vector3.up));

            if (_straightMovement == null)
            {
                _straightMovement = GetComponent<NgoDarkWizardSectorAttackStraightMovement>();
            }

            if (_collisionBehaviour == null)
            {
                _collisionBehaviour = GetComponent<NgoDarkWizardSectorAttackCollisionBehaviour>();
            }

            if (networkParams.ArgUlong != ulong.MaxValue &&
                _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(networkParams.ArgUlong, out NetworkObject attackerObject))
            {
                BossDarkWizardStats attacker = attackerObject.GetComponent<BossDarkWizardStats>();
                if (_collisionBehaviour != null)
                {
                    _collisionBehaviour.Initialize(attacker);
                }
            }

            if (_straightMovement != null)
            {
                _straightMovement.Initialize(networkParams.ArgFloat);
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardSectorAttack";
        public override int PoolingCapacity => 5;
    }
}
