using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.RelayManager;
using NetWork.Boss_NGO;
using NetWork.NGO.Scene_NGO;
using Scene;
using UI.Scene.SceneUI;
using UnityEngine.SceneManagement;
using Zenject;

namespace BehaviourTreeNode.BossGolem.Task
{
    public class MovePlayGameScene : Action
    {
         private RelayManager _relayManager;
        private RelayManager RelayManager
        {
            get
            {
                if (_relayManager == null)
                {
                    _relayManager = GetComponent<BossDependencyHub>().RelayManager;
                }
                return _relayManager;
            }
        }
        private NgoStageTimerController _ngoStageTimerController;
        private BehaviorTree _tree;
        public override void OnStart()
        {
            base.OnStart();
            GetComponent<BossGolemAnimationNetworkController>().RemoveBossHpBarRpc();
            _ngoStageTimerController = RelayManager.SpawnNetworkObj("Prefabs/NGO/Scene_NGO/NgoStageTimerController").GetComponent<NgoStageTimerController>();
            _tree = Owner.GetComponent<BehaviorTree>();
        }
        public override TaskStatus OnUpdate()
        {
            if (_tree == null) return TaskStatus.Running;
            _tree.DisableBehavior(); // 내부적으로 정리하면서 비활성화
            return TaskStatus.Success;
        }
        
    }
}
