using GameManagers.SoundManagement;
using UnityEngine;
using Zenject;

namespace Module.EnemyModule.Boss.DarkWizard
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UISoundBinder))]
    [RequireComponent(typeof(SoundPlayerBinder))]
    public sealed class DarkWizardSoundAnimationEvent : MonoBehaviour
    {
        private const string AttackCueId = "DrakWizardAttackSFX";
        private const string OpenningCueId = "DarkWizardOpenningSFX";
        private const string SpawnMinionCueId = "SpawnMinionSFX";
        private const string Skill2CueId = "DarkWizardSkill2SFX";

        [Inject]
        private ISoundManagerServices _soundManagerServices;

        private SoundPlayerBinder _soundPlayerBinder;

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public void DarkWizardAttackSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(AttackCueId);
        }

        public void DarkWizardOpenningSfxEvent()
        {
            _soundManagerServices.PlayUiSfx(gameObject, OpenningCueId);
        }

        public void DarkWizardSpawnMinionSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(SpawnMinionCueId);
        }

        public void DarkWizardSkill2SfxEvent()
        {
            _soundPlayerBinder.PlayDetached(Skill2CueId);
        }
    }
}
