using System;
using System.Collections;
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork.BaseNGO;
using NetWork.NGO;
using Stats;
using Stats.BaseStats;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Attack.Mage
{
    public class NgoMageAttackInitialize : NgoPoolingInitializeBase
    {
        public class MageAttackFactory : NgoZenjectFactory<NgoMageAttackInitialize>, IMageFactoryMarker
        {
            public MageAttackFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Mage/MageAttack");
            }
        }

        private LayerMask _collisionLayer = default;
        private IResourcesServices _resources;
        private PlayerStats _caller;

        [Inject]
        public void Construct(IResourcesServices resources)
        {
            _resources = resources;
        }


        private void Awake()
        {
            _collisionLayer = (1 << LayerMask.NameToLayer("Monster")) | (1 << LayerMask.NameToLayer("Ground"));
        }

        private float _moveSpeed = 8f;
        private float _hitRadius = 1f;


        /// <summary>
        /// 결정론적 로직을 사용 스폰될때 위치 방향 속도의 정보만 담고
        /// 화살이 나가갈땐 각 로컬에서 계산해서 화살이 나아가는 걸 계산하고
        /// 충돌처리는 전적으로 서버가 담당함.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            StartCoroutine(Move(gameObject));
            _resources.DestroyObject(gameObject, 4f); //무한히 날아가는걸 방지
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            transform.position = targetGo.transform.position
                                 + (targetGo.transform.forward)
                                 + (targetGo.transform.up * 0.5f);
            transform.rotation = targetGo.transform.rotation;

            _caller = targetGo.GetComponent<PlayerStats>();
            Debug.Assert(_caller != null, "_caller is null check the Scripts");
        }

        IEnumerator Move(GameObject go)
        {
            // 화살의 전방 방향
            Vector3 direction = Vector3.forward;

            while (true)
            {
                // 이번 프레임에 이동할 거리 계산
                float moveDistance = _moveSpeed * Time.deltaTime;

                // 1. 이동하기 전에 Raycast로 경로상의 충돌 체크 (터널링 방지)
                // transform.TransformDirection(Vector3.forward)는 현재 객체의 로컬 앞쪽을 월드 방향으로 변환

                // 중요: 서버만 충돌체크
                if (IsServer)
                {
                    if (CheckCollision(go.transform.position, go.transform.forward, moveDistance))
                    {
                        // 충돌했으면 이동 멈춤
                        yield break;
                    }
                }

                // 2. 충돌이 없으면 이동
                go.transform.Translate(direction * moveDistance);

                yield return null;
            }
        }

        /// <summary>
        /// 레이캐스트를 이용한 충돌 감지 로직
        /// </summary>
        private bool CheckCollision(Vector3 origin, Vector3 direction, float distance)
        {
            RaycastHit hit;

            // 현재 위치에서 진행 방향으로 이동할 거리만큼 Ray 발사
            if (Physics.SphereCast(origin, _hitRadius, direction, out hit, distance, _collisionLayer))
            {
                OnHit(hit);
                return true;
            }

            return false;
        }

        private void OnHit(RaycastHit hit)
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable) == true)
            {
                damageable.OnAttacked(_caller, _caller.Attack);
            }

            _resources.DestroyObject(gameObject);
            _vfxManager.InstantiateParticleInArea(NgoMageAttackHitPath, gameObject.transform.position);
        }

        private string NgoMageAttackHitPath => "Prefabs/Player/VFX/Mage/MageAttackHit";
        public override string PoolingNgoPath => "Prefabs/Player/VFX/Mage/MageAttack";
        public override int PoolingCapacity => 5;
    }
}