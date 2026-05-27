using BehaviorDesigner.Runtime.Tasks;
using Controller.BossState.BossDarkWizard;
using Data;
using UnityEngine;

namespace BehaviourTreeNode.DarkWizard.Task
{
    [TaskCategory("CustomNode/DarkWizard")]
    public class BossDarkWizardSpawnMinion : Action
    {
        private BossDarkWizardController _bossController;

        public override void OnAwake()
        {
            base.OnAwake();
            _bossController = GetComponent<BossDarkWizardController>();
        }

        public override void OnStart()
        {
            base.OnStart();
            _bossController.UpdateStateCast();
        }

        public override TaskStatus OnUpdate()
        {
            if (_bossController.IsAnimationDone(BossDarkWizardAnimHash.DarkWizardCastSpell) == false)
            {
                return TaskStatus.Running;
            }

            return TaskStatus.Success;
        }
    }
}
