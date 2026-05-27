using GameManagers.SoundManagement;
using UnityEngine;
using Util;

namespace Controller
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SoundPlayerBinder))]
    public class PlayerFootstepSfxEmitter : MonoBehaviour
    {
        private const string DefaultCueId = "FootstepSFX";

        [SerializeField]
        private string _cueId = DefaultCueId;

        [SerializeField]
        [Range(0f, 1f)]
        private float _minimumClipWeight = 0.75f;

        private Animator _animator;
        private SoundPlayerBinder _soundPlayerBinder;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();

            if (_animator == null || _soundPlayerBinder == null)
            {
                UtilDebug.LogError($"[{nameof(PlayerFootstepSfxEmitter)}] Required components are missing on {gameObject.name}.");
                enabled = false;
            }
        }

        public void Step(AnimationEvent animationEvent)
        {
            if (_animator.IsInTransition(0))
            {
                return;
            }

            if (animationEvent.isFiredByAnimator && animationEvent.animatorClipInfo.weight < _minimumClipWeight)
            {
                return;
            }

            _soundPlayerBinder.PlayDetached(_cueId);
        }
    }
}
