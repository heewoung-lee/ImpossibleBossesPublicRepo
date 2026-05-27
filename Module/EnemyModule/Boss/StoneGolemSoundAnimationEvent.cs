using GameManagers.SoundManagement;
using UnityEngine;
using Zenject;

namespace Module.EnemyModule.Boss
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SoundPlayerBinder))]
    public class StoneGolemSoundAnimationEvent : MonoBehaviour
    {
        private const string AttackCueId = "BossGolemAttackSFX";
        private const string DeadCueId = "BossGolemDeadSFX";
        private const string Skill1CueId = "BossSkill1SFX";
        private const string Skill3CueId = "BossSkill3SFX";
        private const string FootStep1CueId = "BossGolemFootStep1SFX";
        private const string FootStep2CueId = "BossGolemFootStep2SFX";
        private const string SpawnMinionCueId = "SpawnMinionSFX";
        private const string WakeUpCueId = "BossGolemWakeUpSFX";

        [Inject]
        private ISoundManagerServices _soundManagerServices;

        private Animator _animator;
        private SoundPlayerBinder _soundPlayerBinder;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public void BossGolemFootStep1SfxEvent()
        {
            // Move -> Attack 같은 blend 중에도 이동 애니메이션 이벤트가 늦게 들어올 수 있어서,
            // 전환 중에는 이전 상태의 발소리가 겹쳐 나지 않게 막는다.
            if (_animator.IsInTransition(0))
            {
                return;
            }

            _soundPlayerBinder.PlayDetached(FootStep1CueId);
        }

        public void BossGolemFootStep2SfxEvent()
        {
            // Move -> Attack 같은 blend 중에도 이동 애니메이션 이벤트가 늦게 들어올 수 있어서,
            // 전환 중에는 이전 상태의 발소리가 겹쳐 나지 않게 막는다.
            if (_animator.IsInTransition(0))
            {
                return;
            }

            _soundPlayerBinder.PlayDetached(FootStep2CueId);
        }

        public void BossGolemWakeUpSfxEvent()
        {
            _soundManagerServices.PlayUiSfx(gameObject, WakeUpCueId);
        }

        public void BossGolemAttackSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(AttackCueId);
        }

        public void BossGolemDeadSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(DeadCueId);
        }

        public void BossGolemSkill1SfxEvent()
        {
            _soundPlayerBinder.PlayDetached(Skill1CueId);
        }

        public void BossGolemSkill3SfxEvent()
        {
            _soundPlayerBinder.PlayDetached(Skill3CueId);
        }

        public void BossGolemSpawnMinionSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(SpawnMinionCueId);
        }
    }
}
