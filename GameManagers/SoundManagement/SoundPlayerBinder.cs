using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers.SoundManagement
{
    [Serializable]
    public sealed class SoundCueEntry
    {
        [SerializeField, LabelText("Cue")]
        private string _id;

        [SerializeField, LabelText("Clip")]
        private AudioClip _clip;

        [SerializeField, LabelText("Playback")]
        private SoundPlaybackSettings _playbackSettings = new SoundPlaybackSettings();

        public string Id => _id;
        public AudioClip Clip => _clip;
        public SoundPlaybackSettings PlaybackSettings => _playbackSettings;
    }

    [DisallowMultipleComponent]
    public class SoundPlayerBinder : MonoBehaviour
    {
        [SerializeField, ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        private List<SoundCueEntry> _cues = new List<SoundCueEntry>();

        private readonly Dictionary<string, SoundCueEntry> _cueMap = new Dictionary<string, SoundCueEntry>();
        private readonly Dictionary<string, PooledSoundEmitter> _loopingCueMap = new Dictionary<string, PooledSoundEmitter>();
        private ISoundManagerServices _soundManagerServices;

        [Inject]
        public void Construct(ISoundManagerServices soundManagerServices)
        {
            _soundManagerServices = soundManagerServices;
        }

        private void Awake()
        {
            BuildCueMap();
        }

        private void OnDisable()
        {
            StopAllLoops();
        }

        public void Play(string cueId)
        {
            if (TryGetCue(cueId, out SoundCueEntry cue) == false)
            {
                return;
            }

            _soundManagerServices.PlaySfxAtTarget(cue.Clip, transform, cue.PlaybackSettings);
        }

        public void PlayDetached(string cueId)
        {
            if (TryGetCue(cueId, out SoundCueEntry cue) == false)
            {
                return;
            }

            _soundManagerServices.PlaySfxAtPosition(cue.Clip, transform.position, cue.PlaybackSettings);
        }

        public void PlayDetached(string cueId, float pitch)
        {
            if (TryGetCue(cueId, out SoundCueEntry cue) == false)
            {
                return;
            }

            _soundManagerServices.PlaySfxAtPosition(cue.Clip, transform.position, cue.PlaybackSettings, pitch);
        }

        public void PlayDetached(string cueId, Vector3 position)
        {
            if (TryGetCue(cueId, out SoundCueEntry cue) == false)
            {
                return;
            }

            _soundManagerServices.PlaySfxAtPosition(cue.Clip, position, cue.PlaybackSettings);
        }

        public void PlayLoop(string cueId)
        {
            PlayLoop(cueId, 1f);
        }

        public void PlayLoop(string cueId, float pitch)
        {
            if (TryGetCue(cueId, out SoundCueEntry cue) == false)
            {
                return;
            }

            if (_loopingCueMap.TryGetValue(cueId, out PooledSoundEmitter existingLoopHandle) &&
                existingLoopHandle != null &&
                existingLoopHandle.IsUsing)
            {
                return;
            }

            PooledSoundEmitter loopingHandle =
                _soundManagerServices.PlaySfxAtTarget(cue.Clip, transform, cue.PlaybackSettings, true);

            if (loopingHandle == null)
            {
                _loopingCueMap.Remove(cueId);
                return;
            }

            loopingHandle.GetComponent<AudioSource>().pitch = Mathf.Max(pitch, 0.01f);
            _loopingCueMap[cueId] = loopingHandle;
        }

        public void StopLoop(string cueId)
        {
            if (_loopingCueMap.TryGetValue(cueId, out PooledSoundEmitter loopHandle) == false)
            {
                return;
            }

            if (loopHandle != null && loopHandle.IsUsing)
            {
                loopHandle.Push();
            }

            _loopingCueMap.Remove(cueId);
        }

        public bool TryGetClip(string cueId, out AudioClip clip)
        {
            clip = null;

            if (TryGetCue(cueId, out SoundCueEntry cue) == false)
            {
                return false;
            }

            clip = cue.Clip;
            return true;
        }

        private bool TryGetCue(string cueId, out SoundCueEntry cue)
        {
            cue = null;

            if (_soundManagerServices == null)
            {
                UtilDebug.LogError(
                    $"[{nameof(SoundPlayerBinder)}] {nameof(ISoundManagerServices)} was not injected on {gameObject.name}.");
                return false;
            }

            if (string.IsNullOrEmpty(cueId))
            {
                UtilDebug.LogError($"[{nameof(SoundPlayerBinder)}] Cue ID is empty on {gameObject.name}.");
                return false;
            }

            if (_cueMap.TryGetValue(cueId, out cue) == false)
            {
                UtilDebug.LogError($"[{nameof(SoundPlayerBinder)}] Cue '{cueId}' was not found on {gameObject.name}.");
                return false;
            }

            return true;
        }

        private void BuildCueMap()
        {
            _cueMap.Clear();

            if (_cues == null)
            {
                return;
            }

            for (int cueIndex = 0; cueIndex < _cues.Count; cueIndex++)
            {
                SoundCueEntry cue = _cues[cueIndex];

                if (string.IsNullOrEmpty(cue.Id))
                {
                    UtilDebug.LogError($"[{nameof(SoundPlayerBinder)}] Cue ID is empty at index {cueIndex} on {gameObject.name}.");
                    continue;
                }

                if (_cueMap.ContainsKey(cue.Id))
                {
                    UtilDebug.LogError($"[{nameof(SoundPlayerBinder)}] Cue ID '{cue.Id}' is duplicated on {gameObject.name}.");
                    continue;
                }

                if (cue.PlaybackSettings == null)
                {
                    UtilDebug.LogError($"[{nameof(SoundPlayerBinder)}] Cue '{cue.Id}' does not contain {nameof(SoundPlaybackSettings)} on {gameObject.name}.");
                    continue;
                }

                if (cue.Clip == null)
                {
                    UtilDebug.LogError($"[{nameof(SoundPlayerBinder)}] Cue '{cue.Id}' does not have an AudioClip on {gameObject.name}.");
                    continue;
                }

                _cueMap.Add(cue.Id, cue);
            }
        }

        private void StopAllLoops()
        {
            if (_loopingCueMap.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, PooledSoundEmitter> loopHandlePair in _loopingCueMap)
            {
                PooledSoundEmitter loopHandle = loopHandlePair.Value;
                if (loopHandle != null && loopHandle.IsUsing)
                {
                    loopHandle.Push();
                }
            }

            _loopingCueMap.Clear();
        }
    }
}
