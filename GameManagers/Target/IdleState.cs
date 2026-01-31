namespace GameManagers.Target
{
    public class IdleState : ITargetingState
    {
        public void Enter()
        {
            
        }
        public void Update() {}
        public void Exit() {}
        public void OnCancel() {}
        public bool IsComplete { get; set; } = false;
    }
}