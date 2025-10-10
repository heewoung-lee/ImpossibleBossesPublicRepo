using System.Collections;
using System.Resources;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using NetWork;
using NetWork.NGO.Interface;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Skill.AllofSkills.BossMonster.StoneGolem
{
    public class StoneGolemSkill1StoneInitialize : MonoBehaviour, ISpawnBehavior
    {
        public class StoneGolemSkill1StoneFactory : GameObjectContextFactory<StoneGolemSkill1StoneInitialize>
        {
            [Inject]
            public StoneGolemSkill1StoneFactory(DiContainer container, IResourcesServices loadService,
                IFactoryController registerableFactory) : base(container, loadService, registerableFactory)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/BossSkill1");
            }
        }


        private IResourcesServices _resourceManager;
        private IBossSpawnManager _bossSpawnManager;
        private IVFXManagerServices _vfxManager;

        [Inject]
        public void Construct(IResourcesServices resourceManager, IBossSpawnManager bossSpawnManager,
            IVFXManagerServices vfxManager)
        {
            _resourceManager = resourceManager;
            _bossSpawnManager = bossSpawnManager;
            _vfxManager = vfxManager;
        }

        private const float MaxHeight = 3f;
        private const int FlightdurationTime = 1;

        public void SpawnObjectToLocal(in SpawnParamBase stoneParams, string runtimePath = null)
        {
            transform.SetParent(_vfxManager.VFXRoot, false);

            Collider bossTr = _bossSpawnManager.GetBossMonster().transform.GetComponent<Collider>();
            transform.position = bossTr.transform.position + Vector3.up * bossTr.GetComponent<Collider>().bounds.max.y;
            transform.rotation = Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
            Vector3 targetPos = stoneParams.ArgPosVector3;
            StartCoroutine(ThrowStoneParabola(transform, targetPos, FlightdurationTime));
        }

        public IEnumerator ThrowStoneParabola(Transform projectile, Vector3 targetPlayer, float duration)
        {
            Vector3 startPoint = projectile.transform.position;
            Vector3 targetPoint = targetPlayer;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                // t: 진행 비율 (0~1)
                float t = elapsedTime / duration;

                // XZ 위치 보간
                Vector3 currentXZ = Vector3.Lerp(startPoint, targetPoint, t);

                // Y 값은 포물선 계산
                float currentY = Mathf.Lerp(startPoint.y, targetPoint.y, t) + MaxHeight * Mathf.Sin(Mathf.PI * t);

                // 최종 위치 설정
                projectile.position = new Vector3(currentXZ.x, currentY, currentXZ.z);

                yield return null;
            }

            // 포물선 이동 완료 후 파괴
            _resourceManager.DestroyObject(projectile.gameObject, 2f);
        }
    }
}