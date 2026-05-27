using System.Collections;
using GameManagers.GameManagerExManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using Module.EnemyModule;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene;
using Stats;
using UI.Scene.SceneUI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace ScenesScripts.CommonInstaller
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseScene))]
    // This coordinator owns only the opening gates.
    // Timeline owns the actual cinematic direction.
    public class BossSceneOpeningCinematicCoordinator : MonoBehaviour
    {
        private IUIManagerServices _uiManagerServices;
        private IResourcesServices _resourcesServices;
        private IPlayerSpawnManager _playerSpawnManager;
        private IBossSpawnManager _bossSpawnManager;
        private RelayManager _relayManager;
        private SignalBus _signalBus;
        private CinemachineCamera _playerFollowingCamera;

        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;
        private IBossSpawnCinematicTarget _bossTarget;
        private PlayableDirector _openingPlayableDirector;
        private BossFocusCameraTravelDriver _bossFocusCameraTravelDriver;
        private Coroutine _waitAndPlayTimelineCoroutine;

        private bool _isBossAnimationReady;
        private bool _isLoadingCompleted;
        private bool _hasReportedOpeningReady;
        private bool _hasStartedOpeningSequence;

        public bool HasReportedOpeningReady => _hasReportedOpeningReady;

        [Inject]
        public void Construct(
            IUIManagerServices uiManagerServices,
            IResourcesServices resourcesServices,
            IPlayerSpawnManager playerSpawnManager,
            IBossSpawnManager bossSpawnManager,
            RelayManager relayManager,
            SignalBus signalBus,
            CinemachineCamera playerFollowingCamera)
        {
            _uiManagerServices = uiManagerServices;
            _resourcesServices = resourcesServices;
            _playerSpawnManager = playerSpawnManager;
            _bossSpawnManager = bossSpawnManager;
            _relayManager = relayManager;
            _signalBus = signalBus;
            _playerFollowingCamera = playerFollowingCamera;
        }

        private void OnEnable()
        {
            BindLoadingProgress();
            BindPlayableDirector();
            _bossSpawnManager.OnBossSpawnEvent += HandleBossSpawned;
            _signalBus.Subscribe<BossAnimationNetworkReadySignal>(HandleBossAnimationNetworkReady);
            _signalBus.Subscribe<BossSceneOpeningStartSignal>(HandleBossSceneOpeningStart);
            TryCacheBossTarget();
        }

        private void OnDisable()
        {
            UnbindLoadingProgress();
            UnbindPlayableDirector();
            _bossSpawnManager.OnBossSpawnEvent -= HandleBossSpawned;
            _signalBus.Unsubscribe<BossAnimationNetworkReadySignal>(HandleBossAnimationNetworkReady);
            _signalBus.Unsubscribe<BossSceneOpeningStartSignal>(HandleBossSceneOpeningStart);
            StopScheduledOpeningTimeline();
        }

        private void BindLoadingProgress()
        {
            if (_gamePlaySceneLoadingProgress != null)
            {
                return;
            }

            UILoading uiLoading = _uiManagerServices.GetOrCreateSceneUI<UILoading>();
            _gamePlaySceneLoadingProgress =
                _resourcesServices.GetOrAddComponent<GamePlaySceneLoadingProgress>(uiLoading.gameObject);

            _gamePlaySceneLoadingProgress.OnLoadingFadeOutStart += HandleLoadingFadeOutStart;
            _gamePlaySceneLoadingProgress.OnLoadingComplete += HandleLoadingComplete;

            if (uiLoading.gameObject.activeSelf == false)
            {
                HandleLoadingComplete();
            }
        }

        private void UnbindLoadingProgress()
        {
            if (_gamePlaySceneLoadingProgress == null)
            {
                return;
            }

            _gamePlaySceneLoadingProgress.OnLoadingFadeOutStart -= HandleLoadingFadeOutStart;
            _gamePlaySceneLoadingProgress.OnLoadingComplete -= HandleLoadingComplete;
            _gamePlaySceneLoadingProgress = null;
        }

        private void BindPlayableDirector()
        {
            if (_openingPlayableDirector != null)
            {
                return;
            }

            _openingPlayableDirector = GameObject.FindAnyObjectByType<PlayableDirector>();
            _bossFocusCameraTravelDriver = GameObject.FindAnyObjectByType<BossFocusCameraTravelDriver>();
            SetBossFocusCameraLive(false);
            _openingPlayableDirector.stopped += HandleOpeningPlayableDirectorStopped;
        }

        private void UnbindPlayableDirector()
        {
            if (_openingPlayableDirector == null)
            {
                return;
            }

            _openingPlayableDirector.stopped -= HandleOpeningPlayableDirectorStopped;
            _openingPlayableDirector = null;
            SetBossFocusCameraLive(false);
            _bossFocusCameraTravelDriver = null;
        }

        private void HandleLoadingFadeOutStart()
        {
            BindPlayableDirector();
            SetBossFocusCameraLive(true);
            _bossFocusCameraTravelDriver.SnapToScenePose();
        }

        private void HandleLoadingComplete()
        {
            if (_isLoadingCompleted)
            {
                return;
            }

            _isLoadingCompleted = true;
            BindPlayableDirector();
            SetBossFocusCameraLive(true);
            _bossFocusCameraTravelDriver.SnapToScenePose();
            TryCacheBossTarget();
            TryReportOpeningReady();
        }

        private void HandleBossSpawned()
        {
            TryCacheBossTarget();
        }

        private void TryCacheBossTarget()
        {
            GameObject bossMonster = _bossSpawnManager.GetBossMonster();
            if (bossMonster == null)
            {
                return;
            }

            IBossSpawnCinematicTarget nextTarget = bossMonster.GetComponent<IBossSpawnCinematicTarget>();
            if (nextTarget == null)
            {
                return;
            }

            if (ReferenceEquals(_bossTarget, nextTarget))
            {
                return;
            }

            _bossTarget = nextTarget;
            _isBossAnimationReady = false;
            _hasReportedOpeningReady = false;
        }

        private void TryReportOpeningReady()
        {
            if (_isLoadingCompleted == false || _hasReportedOpeningReady)
            {
                return;
            }

            if (_bossTarget == null || _isBossAnimationReady == false)
            {
                return;
            }

            _hasReportedOpeningReady = true;
            _signalBus.Fire(new BossSceneOpeningLocalReadySignal());
        }

        private void HandleBossAnimationNetworkReady(BossAnimationNetworkReadySignal signal)
        {
            if (signal == null || signal.BossMonster == null)
            {
                return;
            }

            IBossSpawnCinematicTarget nextTarget = signal.BossMonster.GetComponent<IBossSpawnCinematicTarget>();
            if (nextTarget == null)
            {
                return;
            }

            _bossTarget = nextTarget;
            _isBossAnimationReady = true;
            _bossTarget.OnSpawnedForCinematic();
            TryReportOpeningReady();
        }

        private void HandleBossSceneOpeningStart(BossSceneOpeningStartSignal signal)
        {
            if (signal == null || _hasStartedOpeningSequence)
            {
                return;
            }

            NotifySceneOpeningStart();
            StopScheduledOpeningTimeline();
            _waitAndPlayTimelineCoroutine = StartCoroutine(WaitAndPlayTimelineRoutine(signal.StartServerTime));
        }

        private IEnumerator WaitAndPlayTimelineRoutine(double startServerTime)
        {
            while (_relayManager.NetworkManagerEx.ServerTime.Time < startServerTime)
            {
                yield return null;
            }

            _waitAndPlayTimelineCoroutine = null;
            BindPlayableDirector();
            _hasStartedOpeningSequence = true;
            SetBossFocusCameraLive(true);
            _bossFocusCameraTravelDriver.PrepareTravel();
            _openingPlayableDirector.time = 0d;
            _openingPlayableDirector.Evaluate();
            _openingPlayableDirector.Play();
        }

        private void StopScheduledOpeningTimeline()
        {
            if (_waitAndPlayTimelineCoroutine == null)
            {
                return;
            }

            StopCoroutine(_waitAndPlayTimelineCoroutine);
            _waitAndPlayTimelineCoroutine = null;
        }

        private void HandleOpeningPlayableDirectorStopped(PlayableDirector stoppedDirector)
        {
            if (_openingPlayableDirector == null || stoppedDirector != _openingPlayableDirector)
            {
                return;
            }

            if (_hasStartedOpeningSequence == false)
            {
                return;
            }

            SetBossFocusCameraLive(false);
            _bossSpawnManager.GetBossMonster().GetComponent<ModuleBossHpUI>().ShowBossHpUI();
            _bossTarget.OnCombatStart();
            NotifySceneOpeningEnd();
            _hasStartedOpeningSequence = false;
        }

        private void SetBossFocusCameraLive(bool isBossFocusLive)
        {
            if (_bossFocusCameraTravelDriver == null)
            {
                return;
            }

            CinemachineCamera bossFocusCamera = _bossFocusCameraTravelDriver.GetComponent<CinemachineCamera>();
            if (isBossFocusLive)
            {
                if (_playerFollowingCamera != null)
                {
                    _playerFollowingCamera.enabled = false;
                }

                bossFocusCamera.enabled = true;
                return;
            }

            if (_playerFollowingCamera != null)
            {
                _playerFollowingCamera.enabled = true;
            }

            bossFocusCamera.enabled = false;
        }

        public void PlayBossCinematicAnimation()
        {
            _bossTarget.OnCinematicStart();
        }

        public void NotifySceneOpeningStart()
        {
            GameObject player = _playerSpawnManager.GetPlayer();
            IPlayerSceneOpeningTarget target = player != null ? player.GetComponent<IPlayerSceneOpeningTarget>() : null;
            if (target != null)
            {
                target.OnSceneOpeningStart();
                return;
            }

            void OnPlayerSpawned(PlayerStats playerStats)
            {
                _playerSpawnManager.OnPlayerSpawnEvent -= OnPlayerSpawned;
                IPlayerSceneOpeningTarget spawnedTarget =
                    playerStats != null ? playerStats.GetComponent<IPlayerSceneOpeningTarget>() : null;
                if (spawnedTarget != null)
                {
                    spawnedTarget.OnSceneOpeningStart();
                }
            }

            _playerSpawnManager.OnPlayerSpawnEvent -= OnPlayerSpawned;
            _playerSpawnManager.OnPlayerSpawnEvent += OnPlayerSpawned;
        }

        public void NotifySceneOpeningEnd()
        {
            GameObject player = _playerSpawnManager.GetPlayer();
            IPlayerSceneOpeningTarget target = player != null ? player.GetComponent<IPlayerSceneOpeningTarget>() : null;
            if (target != null)
            {
                target.OnSceneOpeningEnd();
                return;
            }

            void OnPlayerSpawned(PlayerStats playerStats)
            {
                _playerSpawnManager.OnPlayerSpawnEvent -= OnPlayerSpawned;
                IPlayerSceneOpeningTarget spawnedTarget =
                    playerStats != null ? playerStats.GetComponent<IPlayerSceneOpeningTarget>() : null;
                if (spawnedTarget != null)
                {
                    spawnedTarget.OnSceneOpeningEnd();
                }
            }

            _playerSpawnManager.OnPlayerSpawnEvent -= OnPlayerSpawned;
            _playerSpawnManager.OnPlayerSpawnEvent += OnPlayerSpawned;
        }
    }
}
