using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using Stats.MonsterStats;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss.Darkwizard.Minion.MinionGolem
{
    public class NgoMinionGolemAttackInitialize : NgoPoolingInitializeBase
    {
        private NgoMinionGolemAttackBehaviour _attackBehaviour;
        private MinionGolemStats _attacker;

        public class NgoMinionGolemAttackFactory : NgoZenjectFactory<NgoMinionGolemAttackInitialize>
        {
            public NgoMinionGolemAttackFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Minion/MinionGolemAttack");
            }
        }

        private void Awake()
        {
            _attackBehaviour = GetComponent<NgoMinionGolemAttackBehaviour>();
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);

            _attacker = targetGo.GetComponent<MinionGolemStats>();
            if (_attacker == null)
            {
                Util.UtilDebug.LogWarning("MinionGolemStats is missing on targetGo.");
                return;
            }

            Vector3 fireDirection = transform.forward;
            if (fireDirection.sqrMagnitude <= 0.0001f)
            {
                fireDirection = Vector3.forward;
            }
            else
            {
                fireDirection.Normalize();
            }

            transform.position = targetGo.transform.position + fireDirection * _attacker.ProjectileSpawnDistance;
            transform.position += Vector3.up * _attacker.ProjectileSpawnHeight;
            transform.rotation = Quaternion.LookRotation(fireDirection, Vector3.up);

            if (_attackBehaviour == null)
            {
                _attackBehaviour = GetComponent<NgoMinionGolemAttackBehaviour>();
            }

            if (_attackBehaviour != null)
            {
                _attackBehaviour.Initialize(_attacker, fireDirection);
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Minion/MinionGolemAttack";
        public override int PoolingCapacity => 400;
    }
}
