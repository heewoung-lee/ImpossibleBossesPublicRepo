using System;
using Controller.ControllerStats;

namespace Controller.BossState.BossGolem
{
    public class BossSpawnRockState : IState
    {
        public bool LockAnimationChange => false;

        public event Action UpdateStateEvent;

        public BossSpawnRockState(Action bossSpawnRockState)
        {
            UpdateStateEvent += bossSpawnRockState;
        }

        public void UpdateState()
        {
            UpdateStateEvent?.Invoke();
        }
    }
}
