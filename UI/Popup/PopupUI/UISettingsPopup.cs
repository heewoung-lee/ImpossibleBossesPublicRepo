using GameManagers.SoundManagement;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UISettingsPopup : UIPopup
    {
        private enum VolumeGroups
        {
            MainVolume,
            BGMVolume,
            SFXVolume
        }

        private enum Buttons
        {
            ApplyButton
        }

        private Slider _masterVolumeSlider;
        private Slider _bgmVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Button _applyButton;

        protected override void AwakeInit()
        {
            Bind<GameObject>(typeof(VolumeGroups));
            Bind<Button>(typeof(Buttons));

            _masterVolumeSlider = FindVolumeSlider(VolumeGroups.MainVolume);
            _bgmVolumeSlider = FindVolumeSlider(VolumeGroups.BGMVolume);
            _sfxVolumeSlider = FindVolumeSlider(VolumeGroups.SFXVolume);
            _applyButton = GetButton((int)Buttons.ApplyButton);
        }

        protected override void StartInit()
        {
            if (CanBindVolumeSettings() == false)
            {
                return;
            }

            _masterVolumeSlider.onValueChanged.AddListener(ApplyMasterVolume);
            _bgmVolumeSlider.onValueChanged.AddListener(ApplyBgmVolume);
            _sfxVolumeSlider.onValueChanged.AddListener(ApplySfxVolume);
            _applyButton.onClick.AddListener(SaveVolumeSettings);

            SyncVolumeSliders();
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();

            if (_soundManagerServices == null)
            {
                return;
            }

            if (_masterVolumeSlider == null || _bgmVolumeSlider == null || _sfxVolumeSlider == null)
            {
                return;
            }

            SyncVolumeSliders();
        }

        private Slider FindVolumeSlider(VolumeGroups group)
        {
            GameObject groupObject = GetObject((int)group);
            if (groupObject == null)
            {
                UtilDebug.LogError($"[{nameof(UISettingsPopup)}] Failed to find volume group: {group}");
                return null;
            }

            Slider slider = Utill.FindChild<Slider>(groupObject, "Slider", true);
            if (slider == null)
            {
                UtilDebug.LogError($"[{nameof(UISettingsPopup)}] Slider is missing under volume group: {groupObject.name}");
            }

            return slider;
        }

        private bool CanBindVolumeSettings()
        {
            if (_soundManagerServices == null)
            {
                UtilDebug.LogError(
                    $"[{nameof(UISettingsPopup)}] {nameof(ISoundManagerServices)} was not injected on {gameObject.name}.");
                return false;
            }

            if (_masterVolumeSlider == null || _bgmVolumeSlider == null || _sfxVolumeSlider == null)
            {
                return false;
            }

            if (_applyButton == null)
            {
                UtilDebug.LogError($"[{nameof(UISettingsPopup)}] Failed to find ApplyButton on {gameObject.name}.");
                return false;
            }

            return true;
        }

        private void SyncVolumeSliders()
        {
            _masterVolumeSlider.SetValueWithoutNotify(_soundManagerServices.MasterVolume);
            _bgmVolumeSlider.SetValueWithoutNotify(_soundManagerServices.BgmVolume);
            _sfxVolumeSlider.SetValueWithoutNotify(_soundManagerServices.SfxVolume);
        }

        private void ApplyMasterVolume(float volume)
        {
            _soundManagerServices.SetMasterVolume(volume);
        }

        private void ApplyBgmVolume(float volume)
        {
            _soundManagerServices.SetBgmVolume(volume);
        }

        private void ApplySfxVolume(float volume)
        {
            _soundManagerServices.SetSfxVolume(volume);
        }

        private void SaveVolumeSettings()
        {
            _soundManagerServices.SaveVolumeSettings();
        }
    }
}
