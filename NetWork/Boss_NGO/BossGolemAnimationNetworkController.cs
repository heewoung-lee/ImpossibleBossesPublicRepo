using System.Collections.Generic;
using BehaviourTreeNode.BossGolem.Task;
using Controller.BossState;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using UI.Scene.SceneUI;
using Unity.Collections;
using Unity.Netcode;
using Zenject;
using IState = Controller.ControllerStats.IState;

namespace NetWork.Boss_NGO
{
    public class BossGolemAnimationNetworkController : NetworkBehaviour
    {
        private RelayManager _relayManager;
        private IUIManagerServices _uiManager;
        private IResourcesServices _resourcesServices;

        [Inject]
        public void Construct(RelayManager relayManager,IUIManagerServices uiManager, IResourcesServices resourcesServices)
        {
            _relayManager = relayManager;
            _uiManager = uiManager;
            _resourcesServices = resourcesServices;
        }

        BossGolemController _bossGolemController;
        private Dictionary<string,IState> _bossAttackStateDict = new Dictionary<string, IState>();
        
        public Dictionary<string, IState> BossAttackStateDict => _bossAttackStateDict;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _bossGolemController = GetComponent<BossGolemController>();

            foreach(IState istate in _bossGolemController.StateAnimDict.StateDict.Keys)
            {
                string istateName = istate.GetType().Name;
                _bossAttackStateDict.Add(istateName, istate);
            }
        }
        [Rpc(SendTo.ClientsAndHost)]
        private void SetBossStateRpc(FixedString512Bytes stateName)
        {
            _bossGolemController.CurrentStateType = _bossAttackStateDict[stateName.ToString()];
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void RemoveBossHpBarRpc()
        {
            if (_uiManager.Try_Get_Scene_UI(out UIBossHp bossHp))
            {
                _resourcesServices.DestroyObject(bossHp.gameObject);
            }
        }

        public void SyncBossStateToClients<T>(T state) where T : IState
        {
            if (_relayManager.NetworkManagerEx.IsHost == false)
                return;

            string typename = state.GetType().Name;

            SetBossStateRpc(typename);

        }
    }
}
