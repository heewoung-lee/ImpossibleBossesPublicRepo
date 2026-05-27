using UnityEngine;
using Util;
using Zenject;

namespace GameManagers.SoundManagement
{
    [DisallowMultipleComponent]
    public class SceneBgmPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioClip _bgmClip;

        [SerializeField]
        [Range(0f, 1f)]
        private float _bgmVolumeScale = 1f;

        private ISoundManagerServices _soundManagerServices;

        [Inject]
        public void Construct(ISoundManagerServices soundManagerServices)
        {
            _soundManagerServices = soundManagerServices;
        }

        private void Start()
        {
            if (CanPlay() == false)
            {
                return;
            }

            _soundManagerServices.PlayBgm(_bgmClip, _bgmVolumeScale, this);
        }

        private void OnValidate()
        {
            if (ValidateBgmClip(isRuntime: false) == false)
            {
                return;
            }

            if (Application.isPlaying == false || _soundManagerServices == null)
            {
                return;
            }

            _soundManagerServices.UpdateCurrentBgmVolumeScale(this, _bgmVolumeScale);
        }

        private bool CanPlay()
        {
            if (ValidateBgmClip(isRuntime: true) == false)
            {
                return false;
            }

            if (_soundManagerServices == null)
            {
                UtilDebug.LogError(
                    $"[{nameof(SceneBgmPlayer)}] {nameof(ISoundManagerServices)} was not injected on {gameObject.name}.");
                return false;
            }

            return true;
        }

        private bool ValidateBgmClip(bool isRuntime)
        {
            if (_bgmClip != null)
            {
                return true;
            }

            string phase = isRuntime ? "Runtime" : "OnValidate";
            UtilDebug.LogError(
                $"[{nameof(SceneBgmPlayer)}] BGM AudioClip is missing on {gameObject.name} during {phase}. Meta reference may be broken.");
            return false;
        }
    }
}
