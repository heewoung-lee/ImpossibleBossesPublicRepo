using BehaviorDesigner.Runtime.Tasks;
using Unity.Netcode.Components;
using UnityEngine;

namespace BehaviourTreeNode.DarkWizard.Task
{
    [TaskCategory("CustomNode/DarkWizard")]
    public class BossDarkWizardTeleportToCenter : Action
    {
        [SerializeField] private Vector3 _centerPosition = Vector3.zero;

        public override void OnStart()
        {
            base.OnStart();

            Quaternion targetRotation = Quaternion.Euler(0f, 180f, 0f);
            NetworkTransform networkTransform = gameObject.GetComponent<NetworkTransform>();

            if (networkTransform != null)
            {
                networkTransform.Teleport(_centerPosition, targetRotation, transform.localScale);
                return;
            }

            transform.SetPositionAndRotation(_centerPosition, targetRotation);
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}
