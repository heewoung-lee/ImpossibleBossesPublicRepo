using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using Controller.BossState.BossRedDragon;
using Data;

namespace BehaviourTreeNode.CommonNode
{
    [TaskCategory("CustomNode/RedDragon")]
    public class RedDragonProjectileAttack : Action
    {
        private const float ProjectileAttackAnimSpeed = 0.3f;
        private BossRedDragonController _controller;

        public override void OnAwake()
        {
            base.OnAwake();
            _controller = Owner.GetComponent<BossRedDragonController>();
        }

        public override void OnStart()
        {
            base.OnStart();
            _controller.UpdateProjectileAttack();
            _controller.Anim.speed = ProjectileAttackAnimSpeed;
            _controller.SyncCurrentAnimationSpeedOverride(ProjectileAttackAnimSpeed);
        }

        public override TaskStatus OnUpdate()
        {
            return _controller.IsAnimationDone(BossRedDragonAnimHash.RedDragonProjectileAttack)
                ? TaskStatus.Success
                : TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _controller.Anim.speed = 1f;
            _controller.UpdateIdle();
        }
    }
}
