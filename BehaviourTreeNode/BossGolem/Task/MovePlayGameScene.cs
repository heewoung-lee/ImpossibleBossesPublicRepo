using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GameManagers;
using NetWork.NGO.Scene_NGO;
using Scene;
using Zenject;

namespace BehaviourTreeNode.BossGolem.Task
{
    public class MovePlayGameScene : Action
    {
         private SceneManagerEx _sceneManagerEx;
         private RelayManager _relayManager;
        private SceneManagerEx SceneManagerEx
        {
            get
            {
                if (_sceneManagerEx == null)
                {
                    _sceneManagerEx = GetComponent<BossDependencyHub>().SceneManagerEx;
                }
                return _sceneManagerEx;
            }
        }

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
        
        
        private NgoMoveDownTownBehaviour _ngoMoveDownTownBehaviour;
        private NgoStageTimerController _ngoStageTimerController;
        private BehaviorTree _tree;
        private ISceneMover _sceneMover;
        public override void OnStart()
        {
            _ngoMoveDownTownBehaviour = RelayManager.SpawnNetworkObj("Prefabs/NGO/NGO_MoveDownTownBehaviour").GetComponent<NgoMoveDownTownBehaviour>();
            _ngoStageTimerController = RelayManager.SpawnNetworkObj("Prefabs/NGO/Scene_NGO/NgoStageTimerController").GetComponent<NgoStageTimerController>();
            _sceneMover = (SceneManagerEx.GetCurrentScene as IHasSceneMover).SceneMover;//Null 뜨면 에러나야함. Null체크 옵션 사용하면 안됨
            _ngoStageTimerController.UIStageTimer.OnTimerCompleted += _sceneMover.MoveScene;
            _tree = Owner.GetComponent<BehaviorTree>();
            base.OnStart();
        }

        public override TaskStatus OnUpdate()
        {
            if (_tree == null) return TaskStatus.Running;
            _tree.DisableBehavior(); // 내부적으로 정리하면서 비활성화
            RelayManager.DeSpawn_NetWorkOBJ(_ngoMoveDownTownBehaviour.gameObject);
            return TaskStatus.Success;
        }
    }
}
