using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Util;

namespace GameManagers.SoundManagement
{
    public enum UISoundCueIdType
    {
        Common,
        Custom
    }

    [Serializable]
    public sealed class UISoundCue
    {
        [SerializeField]
        private UISoundCueIdType _idType;

        [SerializeField]
        [ShowIf(nameof(IsCommon))]
        private UICommonSoundCueId _commonId;

        [SerializeField]
        [ShowIf(nameof(IsCustom))]
        private string _customId;

        [SerializeField]
        private AudioClip _clip;

        public AudioClip Clip => _clip;

        private bool IsCommon => _idType == UISoundCueIdType.Common;
        private bool IsCustom => _idType == UISoundCueIdType.Custom;

        public bool TryGetCueId(out string cueId)
        {
            cueId = null;

            if (_idType == UISoundCueIdType.Common)
            {
                if (_commonId == UICommonSoundCueId.None)
                {
                    return false;
                }

                cueId = UICommonSoundCueIds.GetId(_commonId);
                return string.IsNullOrEmpty(cueId) == false;
            }

            if (string.IsNullOrEmpty(_customId))
            {
                return false;
            }

            cueId = _customId;
            return true;
        }
    }

    [DisallowMultipleComponent]
    public sealed class UISoundBinder : MonoBehaviour
    {
        [SerializeField]
        private List<UISoundCue> _cues = new List<UISoundCue>();

        private readonly Dictionary<string, AudioClip> _cueMap = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            BuildCueMap();
        }

        public bool TryGetClip(string cueId, out AudioClip clip)
        {
            if (string.IsNullOrEmpty(cueId))
            {
                clip = null;
                return false;
            }

            return _cueMap.TryGetValue(cueId, out clip);
        }

        private void BuildCueMap()
        {
            _cueMap.Clear();

            if (_cues == null)
            {
                return;
            }

            for (int i = 0; i < _cues.Count; i++)
            {
                UISoundCue cue = _cues[i];
                if (cue.TryGetCueId(out string cueId) == false)
                {
                    UtilDebug.LogError($"[{nameof(UISoundBinder)}] Cue ID is invalid at index {i} on {gameObject.name}.");
                    continue;
                }

                if (cue.Clip == null)
                {
                    UtilDebug.LogError($"[{nameof(UISoundBinder)}] Cue '{cueId}' does not contain a valid AudioClip on {gameObject.name}. Meta reference may be broken.");
                    continue;
                }

                if (_cueMap.ContainsKey(cueId))
                {
                    UtilDebug.LogError($"[{nameof(UISoundBinder)}] Cue ID '{cueId}' is duplicated on {gameObject.name}.");
                    continue;
                }

                _cueMap.Add(cueId, cue.Clip);
            }
        }
    }
}
