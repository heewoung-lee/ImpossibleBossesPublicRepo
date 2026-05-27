using GameManagers.GameManagerExManagement;
using UnityEngine;
using Zenject;

namespace ScenesScripts.CommonInstaller
{
    [DisallowMultipleComponent]
    public class BossFocusCameraTargetBinder : MonoBehaviour
    {
        private IBossSpawnManager _bossSpawnManager;

        public Transform CameraAnchor { get; private set; }
        public Transform BossTarget { get; private set; }

        [Inject]
        public void Construct(IBossSpawnManager bossSpawnManager)
        {
            _bossSpawnManager = bossSpawnManager;
        }

        private void Start()
        {
            if (_bossSpawnManager.GetBossMonster() != null)
            {
                BindBossTarget();
                return;
            }

            _bossSpawnManager.OnBossSpawnEvent += HandleBossSpawned;
        }

        private void OnDestroy()
        {
            _bossSpawnManager.OnBossSpawnEvent -= HandleBossSpawned;
        }

        private void HandleBossSpawned()
        {
            BindBossTarget();
        }

        private void BindBossTarget()
        {
            _bossSpawnManager.OnBossSpawnEvent -= HandleBossSpawned;
            Transform bossTransform = _bossSpawnManager.GetBossMonster().transform;
            BossCinematicCameraAnchorMarker cameraAnchor = bossTransform.GetComponentInChildren<BossCinematicCameraAnchorMarker>(true);
            CameraAnchor = cameraAnchor.transform;
            BossTarget = cameraAnchor.LookAtTarget != null
                ? cameraAnchor.LookAtTarget
                : bossTransform;
        }
    }
}
