using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameManagers.PoolManagement;
using GameManagers.ResourcesExManagement;
using Sirenix.OdinInspector;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers.SoundManagement
{
    public interface ISoundManagerServices
    {
        float MasterVolume { get; }
        float BgmVolume { get; }
        float SfxVolume { get; }

        void PlayBgm(AudioClip clip, float volumeScale = 1f, SceneBgmPlayer owner = null);
        void StopBgm();
        void PlayUiSfx(GameObject owner, UICommonSoundCueId cueId);
        void PlayUiSfx(GameObject owner, string cueId);
        void PlaySfxAtPosition(AudioClip clip, Vector3 position, SoundPlaybackSettings settings);
        void PlaySfxAtPosition(AudioClip clip, Vector3 position, SoundPlaybackSettings settings, float pitch);
        void PlaySfxAtTarget(AudioClip clip, Transform target, SoundPlaybackSettings settings);
        PooledSoundEmitter PlaySfxAtTarget(AudioClip clip, Transform target, SoundPlaybackSettings settings, bool loop);
        void UpdateCurrentBgmVolumeScale(SceneBgmPlayer owner, float volumeScale);

        void SetMasterVolume(float volume);
        void SetBgmVolume(float volume);
        void SetSfxVolume(float volume);
        void SaveVolumeSettings();
    }

    public class SoundManager : IInitializable, IResettable, ISoundManagerServices
    {
        private const string SoundRootName = "@Sound_Root";
        private const string BgmObjectName = "BGM";
        private const string RuntimeSfxRootName = "RuntimeSFX";
        private const string PooledEmitterOriginalName = "PooledSoundEmitter";
        private const string UiCommonCueResourceRoot = "Sounds/SFX/UI/Common";
        private const string MasterVolumeKey = "Sound.MasterVolume";
        private const string BgmVolumeKey = "Sound.BGMVolume";
        private const string SfxVolumeKey = "Sound.SFXVolume";
        private const int DefaultEmitterPoolCount = 8;
        private const float DefaultBgmFadeDuration = 1.5f;

        private readonly LocalPoolManager _localPoolManager;
        private readonly IResourcesServices _resourcesServices;
        private readonly List<PooledSoundEmitter> _activeEmitters = new List<PooledSoundEmitter>();

        private Transform _soundRoot;
        private Transform _runtimeSfxRoot;
        private AudioSource _bgmSource;
        private GameObject _emitterOriginal;
        private CancellationTokenSource _bgmTransitionCts;
        private float _bgmVolumeScale = 1f;
        private SceneBgmPlayer _bgmOwner;

        public float MasterVolume { get; private set; } = 1f;
        public float BgmVolume { get; private set; } = 1f;
        public float SfxVolume { get; private set; } = 1f;

        [Inject]
        public SoundManager(LocalPoolManager localPoolManager, IResourcesServices resourcesServices)
        {
            _localPoolManager = localPoolManager;
            _resourcesServices = resourcesServices;
        }

        public void Initialize()
        {
            MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
            SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        }

        public void PlayBgm(AudioClip clip, float volumeScale = 1f, SceneBgmPlayer owner = null)
        {
            if (clip == null)
            {
                return;
            }

            _bgmOwner = owner;
            _bgmVolumeScale = Mathf.Clamp01(volumeScale);

            EnsureRuntimeCreated();

            if (_bgmSource.clip == clip && _bgmSource.isPlaying)
            {
                ApplyBgmVolume();
                return;
            }

            CancellationToken transitionToken = CreateBgmTransitionToken();
            TransitionBgmAsync(clip, transitionToken).Forget();
        }

        public void StopBgm()
        {
            CancelBgmTransition();

            if (_bgmSource == null)
            {
                return;
            }

            _bgmSource.Stop();
            _bgmSource.clip = null;
            _bgmOwner = null;
            _bgmVolumeScale = 1f;
            _bgmSource.volume = GetBgmVolume();
        }

        public void UpdateCurrentBgmVolumeScale(SceneBgmPlayer owner, float volumeScale)
        {
            if (_bgmOwner != owner)
            {
                return;
            }

            _bgmVolumeScale = Mathf.Clamp01(volumeScale);
            ApplyBgmVolume();
        }

        public void PlayUiSfx(GameObject owner, UICommonSoundCueId cueId)
        {
            PlayUiSfx(owner, UICommonSoundCueIds.GetId(cueId));
        }

        // UI는 공용 음원을 주로 사용하므로 기본 재생은 SoundManager가 담당한다.
        // 특정 UI만 다른 음원이 필요할 때만 owner에 UISoundPlayer를 붙여 같은 ID로 override해서 사용한다.
        public void PlayUiSfx(GameObject owner, string cueId)
        {
            if (owner == null)
            {
                UtilDebug.LogError($"[{nameof(SoundManager)}] UI sound owner is null.");
                return;
            }

            if (string.IsNullOrEmpty(cueId))
            {
                UtilDebug.LogError($"[{nameof(SoundManager)}] UI cue ID is empty on {owner.name}.");
                return;
            }

            if (TryGetUiClip(owner, cueId, out AudioClip clip) == false)
            {
                UtilDebug.LogError($"[{nameof(SoundManager)}] UI cue '{cueId}' was not found on {owner.name}.");
                return;
            }

            Play2DSfx(clip);
        }

        public void PlaySfxAtPosition(AudioClip clip, Vector3 position, SoundPlaybackSettings settings)
        {
            PlayEmitter(clip, position, null, settings);
        }

        public void PlaySfxAtPosition(AudioClip clip, Vector3 position, SoundPlaybackSettings settings, float pitch)
        {
            PlayEmitter(clip, position, null, settings, pitch, false);
        }

        public void PlaySfxAtTarget(AudioClip clip, Transform target, SoundPlaybackSettings settings)
        {
            if (target == null)
            {
                UtilDebug.LogError($"[{nameof(SoundManager)}] Target transform is null.");
                return;
            }

            PlayEmitter(clip, target.position, target, settings);
        }

        public PooledSoundEmitter PlaySfxAtTarget(AudioClip clip, Transform target, SoundPlaybackSettings settings, bool loop)
        {
            if (target == null)
            {
                UtilDebug.LogError($"[{nameof(SoundManager)}] Target transform is null.");
                return null;
            }

            if (loop == false)
            {
                PlayEmitter(clip, target.position, target, settings);
                return null;
            }

            return PlayEmitter(clip, target.position, target, settings, 1f, true);
        }

        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            ApplyBgmVolume();
            ApplyActiveEmitterVolumes();
        }

        public void SetBgmVolume(float volume)
        {
            BgmVolume = Mathf.Clamp01(volume);
            ApplyBgmVolume();
        }

        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp01(volume);
            ApplyActiveEmitterVolumes();
        }

        public void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
            PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            ClearActiveEmitters();
        }

        private void EnsureRuntimeCreated()
        {
            if (_soundRoot != null)
            {
                return;
            }

            GameObject soundRootObject = new GameObject(SoundRootName);
            UnityEngine.Object.DontDestroyOnLoad(soundRootObject);
            _soundRoot = soundRootObject.transform;

            GameObject bgmObject = new GameObject(BgmObjectName);
            bgmObject.transform.SetParent(_soundRoot, false);
            _bgmSource = bgmObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.spatialBlend = 0f;

            GameObject runtimeSfxRootObject = new GameObject(RuntimeSfxRootName);
            runtimeSfxRootObject.transform.SetParent(_soundRoot, false);
            _runtimeSfxRoot = runtimeSfxRootObject.transform;

            _emitterOriginal = new GameObject(PooledEmitterOriginalName);
            _emitterOriginal.transform.SetParent(_soundRoot, false);
            _emitterOriginal.SetActive(false);

            AudioSource emitterSource = _emitterOriginal.AddComponent<AudioSource>();
            emitterSource.playOnAwake = false;
            emitterSource.loop = false;
            emitterSource.spatialBlend = 1f;
            emitterSource.rolloffMode = AudioRolloffMode.Logarithmic;
            emitterSource.minDistance = 2f;
            emitterSource.maxDistance = 20f;
            _emitterOriginal.AddComponent<PooledSoundEmitter>();

            _localPoolManager.CreatePool(_emitterOriginal, DefaultEmitterPoolCount);
        }

        private void PlayEmitter(AudioClip clip, Vector3 position, Transform followTarget, SoundPlaybackSettings settings)
        {
            PlayEmitter(clip, position, followTarget, settings, 1f);
        }

        private void PlayEmitter(AudioClip clip, Vector3 position, Transform followTarget, SoundPlaybackSettings settings, float pitch)
        {
            PlayEmitter(clip, position, followTarget, settings, pitch, false);
        }

        private PooledSoundEmitter PlayEmitter(AudioClip clip, Vector3 position, Transform followTarget, SoundPlaybackSettings settings, float pitch, bool loop)
        {
            if (clip == null)
            {
                return null;
            }

            if (settings == null)
            {
                UtilDebug.LogError($"[{nameof(SoundManager)}] {nameof(SoundPlaybackSettings)} is null.");
                return null;
            }

            EnsureRuntimeCreated();
            CleanupEmitters();

            Poolable poolable = _localPoolManager.Pop(_emitterOriginal, _runtimeSfxRoot);
            PooledSoundEmitter emitter = poolable as PooledSoundEmitter;
            if (emitter == null)
            {
                UtilDebug.LogError($"[{nameof(SoundManager)}] {nameof(PooledSoundEmitter)} is missing on pooled emitter.");
                return null;
            }

            float finalVolume = Mathf.Clamp01(MasterVolume * SfxVolume * settings.VolumeScale);
            emitter.PlayClip(clip, position, followTarget, settings, finalVolume, pitch, loop);
            _activeEmitters.Add(emitter);
            return emitter;
        }

        private bool TryGetUiClip(GameObject owner, string cueId, out AudioClip clip)
        {
            clip = null;

            if (owner.TryGetComponent(out UISoundBinder uiSoundBinder) &&
                uiSoundBinder.TryGetClip(cueId, out clip))
            {
                return true;
            }

            string resourceKey = $"{UiCommonCueResourceRoot}/{cueId}";
            return _resourcesServices.TryGetLoad<AudioClip>(resourceKey, out clip);
        }

        private void ApplyBgmVolume()
        {
            if (_bgmSource == null)
            {
                return;
            }

            _bgmSource.volume = GetBgmVolume();
        }

        private void Play2DSfx(AudioClip clip)
        {
            PlayEmitter(clip, Vector3.zero, null, SoundPlaybackSettings.Create2D());
        }

        private float GetBgmVolume()
        {
            return Mathf.Clamp01(MasterVolume * BgmVolume * _bgmVolumeScale);
        }

        private CancellationToken CreateBgmTransitionToken()
        {
            CancelBgmTransition();
            _bgmTransitionCts = new CancellationTokenSource();
            return _bgmTransitionCts.Token;
        }

        private void CancelBgmTransition()
        {
            if (_bgmTransitionCts == null)
            {
                return;
            }

            _bgmTransitionCts.Cancel();
            _bgmTransitionCts.Dispose();
            _bgmTransitionCts = null;
        }

        private async UniTaskVoid TransitionBgmAsync(AudioClip nextClip, CancellationToken cancellationToken)
        {
            try
            {
                if (_bgmSource.isPlaying && _bgmSource.clip != null)
                {
                    float startVolume = _bgmSource.volume;

                    for (float elapsed = 0f; elapsed < DefaultBgmFadeDuration; elapsed += Time.unscaledDeltaTime)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        float t = elapsed / DefaultBgmFadeDuration;
                        _bgmSource.volume = Mathf.Lerp(startVolume, 0f, t);
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    }
                }

                _bgmSource.Stop();
                _bgmSource.clip = nextClip;
                _bgmSource.pitch = 1f;
                _bgmSource.volume = 0f;
                _bgmSource.Play();

                for (float elapsed = 0f; elapsed < DefaultBgmFadeDuration; elapsed += Time.unscaledDeltaTime)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    float t = elapsed / DefaultBgmFadeDuration;
                    _bgmSource.volume = Mathf.Lerp(0f, GetBgmVolume(), t);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                ApplyBgmVolume();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (_bgmTransitionCts != null && _bgmTransitionCts.Token == cancellationToken)
                {
                    _bgmTransitionCts.Dispose();
                    _bgmTransitionCts = null;
                }
            }
        }

        private void ApplyActiveEmitterVolumes()
        {
            CleanupEmitters();

            for (int i = 0; i < _activeEmitters.Count; i++)
            {
                PooledSoundEmitter emitter = _activeEmitters[i];
                if (emitter == null || emitter.IsUsing == false)
                {
                    continue;
                }

                emitter.ApplyVolume(Mathf.Clamp01(MasterVolume * SfxVolume * emitter.VolumeScale));
            }
        }

        private void ClearActiveEmitters()
        {
            for (int i = 0; i < _activeEmitters.Count; i++)
            {
                PooledSoundEmitter emitter = _activeEmitters[i];
                if (emitter == null || emitter.IsUsing == false)
                {
                    continue;
                }

                emitter.Push();
            }

            _activeEmitters.Clear();
        }

        private void CleanupEmitters()
        {
            _activeEmitters.RemoveAll(emitter => emitter == null || emitter.IsUsing == false);
        }
    }

    [Serializable]
    public sealed class SoundPlaybackSettings
    {
        [SerializeField]
        [Range(0f, 1f)]
        [LabelText("Volume")]
        private float _volumeScale = 1f;

        [SerializeField]
        [Min(0f)]
        [LabelText("Min Distance")]
        private float _minDistance = 2f;

        [SerializeField]
        [Min(0.01f)]
        [LabelText("Max Distance")]
        private float _maxDistance = 20f;

        private bool _is2D;

        public float VolumeScale => _volumeScale;
        public float SpatialBlend => _is2D ? 0f : 1f;
        public float MinDistance => _minDistance;
        public float MaxDistance => _maxDistance;
        public AudioRolloffMode RolloffMode => AudioRolloffMode.Logarithmic;
        public bool FollowTarget => _is2D == false;

        public void ClampValues()
        {
            _volumeScale = Mathf.Clamp01(_volumeScale);
            _minDistance = Mathf.Max(0f, _minDistance);
            _maxDistance = Mathf.Max(_minDistance + 0.01f, _maxDistance);
        }

        public static SoundPlaybackSettings Create2D()
        {
            SoundPlaybackSettings settings = new SoundPlaybackSettings
            {
                _volumeScale = 1f,
                _minDistance = 0f,
                _maxDistance = 0.01f,
                _is2D = true
            };

            settings.ClampValues();
            return settings;
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class PooledSoundEmitter : LocalPoolable
    {
        private AudioSource _audioSource;
        private Coroutine _followRoutine;
        private Coroutine _releaseRoutine;
        private Transform _followTarget;
        private float _volumeScale = 1f;

        public float VolumeScale => _volumeScale;
        private AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = GetComponent<AudioSource>();
                }

                return _audioSource;
            }
        }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            WorldPositionStays = false;
        }

        private void OnDisable()
        {
            StopPlaybackState();
        }

        public void PlayClip(AudioClip clip, Vector3 position, Transform followTarget, SoundPlaybackSettings settings, float finalVolume, float pitch, bool loop)
        {
            StopPlaybackState();

            transform.position = position;
            _followTarget = settings.FollowTarget ? followTarget : null;
            _volumeScale = settings.VolumeScale;
            float finalPitch = Mathf.Max(pitch, 0.01f);

            AudioSource.clip = clip;
            AudioSource.playOnAwake = false;
            AudioSource.loop = loop;
            AudioSource.pitch = finalPitch;
            AudioSource.volume = Mathf.Clamp01(finalVolume);
            AudioSource.spatialBlend = settings.SpatialBlend;
            AudioSource.minDistance = settings.MinDistance;
            AudioSource.maxDistance = settings.MaxDistance;
            AudioSource.rolloffMode = settings.RolloffMode;

            if (_followTarget != null)
            {
                _followRoutine = StartCoroutine(FollowTargetRoutine());
            }

            AudioSource.Play();

            if (loop == false)
            {
                _releaseRoutine = StartCoroutine(ReturnAfterPlaybackRoutine((clip.length / finalPitch) + 0.05f));
            }
        }

        public void ApplyVolume(float finalVolume)
        {
            AudioSource.volume = Mathf.Clamp01(finalVolume);
        }

        public override void Push()
        {
            StopPlaybackState();
            base.Push();
        }

        private IEnumerator FollowTargetRoutine()
        {
            while (_followTarget != null)
            {
                transform.position = _followTarget.position;
                yield return null;
            }

            _followRoutine = null;
        }

        private IEnumerator ReturnAfterPlaybackRoutine(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            _releaseRoutine = null;
            Push();
        }

        private void StopPlaybackState()
        {
            if (_followRoutine != null)
            {
                StopCoroutine(_followRoutine);
                _followRoutine = null;
            }

            if (_releaseRoutine != null)
            {
                StopCoroutine(_releaseRoutine);
                _releaseRoutine = null;
            }

            _followTarget = null;

            if (_audioSource == null)
            {
                return;
            }

            _audioSource.Stop();
            _audioSource.clip = null;
            _audioSource.pitch = 1f;
            _audioSource.volume = 1f;
            _audioSource.spatialBlend = 1f;
            _audioSource.minDistance = 2f;
            _audioSource.maxDistance = 20f;
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            _volumeScale = 1f;
        }
    }
}
