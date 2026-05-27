using System.Collections;
using GameManagers;
using GameManagers.GameManagerExManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using UnityEngine;
using Zenject;

namespace Test.TestScripts
{
    public class CanonShooter : MonoBehaviour
    {
        [Inject] private IResourcesServices _resourceManager;
        [Inject] private IPlayerSpawnManager _gameManagerEx;
        [Inject] private IVFXManagerServices _vfxManager;

        public Transform startTransform;
        public Transform targetTransform;
        public GameObject projectilePrefab;
        public float maxHeight = 5f;
        public float flightSpeed = 8f;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Launch();
            }
        }

        private void Start()
        {
            startTransform = transform;
        }

        public void Launch()
        {
            targetTransform = _gameManagerEx.GetPlayer().transform;
            _vfxManager.InstantiateParticleInArea(
                "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoBossSkill1AttackHit",
                targetTransform.position);
        }

        private IEnumerator Parabola(Transform projectile)
        {
            yield break;
        }
    }
}
