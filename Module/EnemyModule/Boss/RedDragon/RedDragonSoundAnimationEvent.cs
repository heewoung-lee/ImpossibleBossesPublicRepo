using GameManagers.SoundManagement;
using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Module.EnemyModule.Boss.RedDragon
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(UISoundBinder))]
    [RequireComponent(typeof(SoundPlayerBinder))]
    public sealed class RedDragonSoundAnimationEvent : NetworkBehaviourBase
    {
        private const string AttackCueId = "RedDragonAttackSFX";
        private const string BreathCueId = "RedDragronBreathSFX";
        private const string BreathStartCueId = "RedDragronBreathStartSFX";
        private const string DeadCueId = "RedDragonDeadSFX";
        private const string FootStep1CueId = "RedDragonFootStep1SFX";
        private const string FootStep2CueId = "RedDragonFootStep2SFX";
        private const string GrowlCueId = "RedDragonGrowlSFX";
        private const string LandingCueId = "RedDragonLandingSFX";
        private const string OpenningCueId = "RedDragonOpenningSFX";
        private const string SpawnMinionCueId = "RedDragonSpawnMinionSFX";
        private const string SpawnProjectileCueId = "RedDragonSpawnProjectileSFX";
        private const string StartFlyCueId = "RedDragoneStartFlySFX";
        private const string TailAttackCueId = "RedDragonTailAttackSFX";

        private Animator _animator;
        private SoundPlayerBinder _soundPlayerBinder;

        [Inject]
        private ISoundManagerServices _soundManagerServices;

        protected override void AwakeInit()
        {
            _animator = GetComponent<Animator>();
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        protected override void StartInit()
        {
        }

        public void RedDragonOpenningSfxEvent()
        {
            _soundManagerServices.PlayUiSfx(gameObject, OpenningCueId);
        }

        public void RedDragonGrowlSfxEvent()
        {
            _soundManagerServices.PlayUiSfx(gameObject, GrowlCueId);
        }

        public void RedDragonSpawnProjectileSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(SpawnProjectileCueId);
        }

        public void RedDragonStartFlySfxEvent()
        {
            _soundPlayerBinder.PlayDetached(StartFlyCueId);
        }

        public void RedDragonLandingSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(LandingCueId);
        }

        public void RedDragonBreathStartSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(BreathStartCueId);
        }

        public void RedDragonSpawnMinionSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(SpawnMinionCueId);
        }

        public void RedDragonDeadSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(DeadCueId);
        }

        public void RedDragonFootStep1SfxEvent()
        {
            if (_animator.IsInTransition(0))
            {
                return;
            }

            _soundPlayerBinder.PlayDetached(FootStep1CueId);
        }

        public void RedDragonFootStep2SfxEvent()
        {
            if (_animator.IsInTransition(0))
            {
                return;
            }

            _soundPlayerBinder.PlayDetached(FootStep2CueId);
        }

        public void PlayTailAttackSfxFromNode()
        {
            PlayNodeSfx(TailAttackCueId);
        }

        public void PlayAttackSfxFromNode()
        {
            PlayNodeSfx(AttackCueId);
        }

        public void PlayBreathSfxFromNode(float duration)
        {
            if (IsServer == false)
            {
                return;
            }

            if (_soundPlayerBinder.TryGetClip(BreathCueId, out AudioClip clip) == false)
            {
                return;
            }

            float pitch = clip.length / Mathf.Max(duration, 0.01f);
            PlayNodeSfxWithPitchRpc(BreathCueId, pitch);
        }

        private void PlayNodeSfx(string cueId)
        {
            if (IsServer == false)
            {
                return;
            }

            PlayNodeSfxRpc(cueId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PlayNodeSfxRpc(string cueId)
        {
            _soundPlayerBinder.PlayDetached(cueId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PlayNodeSfxWithPitchRpc(string cueId, float pitch)
        {
            _soundPlayerBinder.PlayDetached(cueId, pitch);
        }
    }
}
