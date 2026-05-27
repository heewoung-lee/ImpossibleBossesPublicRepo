using System.Collections;
using BehaviorDesigner.Runtime;
using GameManagers.RelayManagement;
using ScenesScripts.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;

namespace Controller.BossState
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class BossSpawnCinematicBehaviour : MonoBehaviour, IBossSpawnCinematicTarget
    {
        [Header("Spawned")]
        [SerializeField] private string _spawnedAnimationStateName = string.Empty;
        [SerializeField, Min(0f)] private float _spawnedTransitionDuration = 0.1f;

        [Header("Cinematic")]
        [SerializeField] private string _cinematicAnimationStateName = string.Empty;
        [SerializeField, Min(0f)] private float _cinematicTransitionDuration = 0.1f;

        [Header("Post Cinematic")]
        [SerializeField] private string _postCinematicAnimationStateName = string.Empty;
        [SerializeField, Min(0f)] private float _postCinematicTransitionDuration = 0.1f;

        [Header("Combat")]
        [SerializeField] private string _combatTriggerEventName = "Combat";

        private BossController _bossController;
        private BehaviorTree _behaviorTree;
        private RelayManager _relayManager;
        private Coroutine _postCinematicTransitionCoroutine;

        [Inject]
        public void Construct(RelayManager relayManager)
        {
            _relayManager = relayManager;
        }

        private void Awake()
        {
            _bossController = GetComponent<BossController>();
            if (_bossController == null)
            {
                throw new MissingComponentException(
                    $"[{nameof(BossSpawnCinematicBehaviour)}] requires a {nameof(BossController)} on the same object.");
            }

            _behaviorTree = GetComponent<BehaviorTree>();
        }

        private void OnDisable()
        {
            StopPostCinematicTransition();
        }

        public void OnSpawnedForCinematic()
        {
            StopPostCinematicTransition();
            ApplyPhase(
                _spawnedAnimationStateName,
                _spawnedTransitionDuration);
        }

        public void OnCinematicStart()
        {
            StopPostCinematicTransition();
            ApplyPhase(
                _cinematicAnimationStateName,
                _cinematicTransitionDuration);
            StartPostCinematicTransition();
        }

        public void OnCombatStart()
        {
            StopPostCinematicTransition();
            ClearBossTargets();

            bool isAuthoritativePeer = _relayManager.NetworkManagerEx.IsHost;
            if (isAuthoritativePeer)
            {
                // Post-cinematic boss state is host-authoritative.
                // If clients force Idle locally here they can overwrite the host's first Move sync.
                _bossController.ForceChangeState(_bossController.BaseIDleState);
                SetBehaviorTreeEnabled(true);
            }

            if (isAuthoritativePeer && _behaviorTree != null && string.IsNullOrWhiteSpace(_combatTriggerEventName) == false)
            {
                _behaviorTree.SendEvent(_combatTriggerEventName);
            }
        }

        private void StartPostCinematicTransition()
        {
            if (string.IsNullOrWhiteSpace(_cinematicAnimationStateName) ||
                string.IsNullOrWhiteSpace(_postCinematicAnimationStateName))
            {
                return;
            }

            _postCinematicTransitionCoroutine = StartCoroutine(WaitForCinematicAnimationEndRoutine());
        }

        private IEnumerator WaitForCinematicAnimationEndRoutine()
        {
            int cinematicAnimationStateHash = Animator.StringToHash(_cinematicAnimationStateName);

            while (isActiveAndEnabled &&
                   _bossController.TryGetCurrentOrNextAnimatorStateInfo(cinematicAnimationStateHash, out _) == false)
            {
                yield return null;
            }

            while (isActiveAndEnabled && _bossController.IsAnimationDone(cinematicAnimationStateHash) == false)
            {
                yield return null;
            }

            _postCinematicTransitionCoroutine = null;
            ApplyPhase(
                _postCinematicAnimationStateName,
                _postCinematicTransitionDuration);
        }

        private void StopPostCinematicTransition()
        {
            if (_postCinematicTransitionCoroutine == null)
            {
                return;
            }

            StopCoroutine(_postCinematicTransitionCoroutine);
            _postCinematicTransitionCoroutine = null;
        }

        private void ApplyPhase(
            string animationStateName,
            float transitionDuration)
        {
            SetBehaviorTreeEnabled(false);
            ClearBossTargets();

            if (string.IsNullOrWhiteSpace(animationStateName))
            {
                return;
            }

            _bossController.RunAnimation(Animator.StringToHash(animationStateName), transitionDuration);
        }

        private void SetBehaviorTreeEnabled(bool isEnabled)
        {
            if (_behaviorTree == null)
            {
                return;
            }

            _behaviorTree.enabled = isEnabled;
        }

        private void ClearBossTargets()
        {
            _bossController.TargetObjectInBehaviourTree = null;
            _bossController.IsTauntedInBehaviourTree = false;
        }
    }
}
