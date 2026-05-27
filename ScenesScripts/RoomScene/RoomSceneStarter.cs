using System;
using GameManagers.RelayManagement;
using GameManagers.UIManagement;
using ScenesScripts.CommonInstaller.Interfaces;
using UI.Scene.SceneUI;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace ScenesScripts.RoomScene
{
    public class RoomSceneStarter : ISceneStarter,IDisposable
    {
        private readonly IUIManagerServices _uiManagerServices;
        private readonly SignalBus _signalBus;
        private readonly RelayManager _relayManager;

        [Inject]
        public RoomSceneStarter(IUIManagerServices uiManagerServices, SignalBus signalBus, RelayManager relayManager)
        {
            _uiManagerServices = uiManagerServices;
            _signalBus = signalBus;
            _relayManager = relayManager;
        }

        public void SceneStart()
        {
            //2.24일 수정 UIRoomCharacterSelect은 로컬UI이기 때문에 미리 로드 
           
            
            if (_relayManager.NetworkManagerEx.IsListening == false)
            {
                _signalBus.Subscribe<RpcCallerReadySignal>(SpwanCharacterSelect);
            }
            else
            {
                SpwanCharacterSelect();
            }
            //TODO: 얘네 둘이 SceneContainer에 의해 생성되어함.
        }

        void SpwanCharacterSelect()
        {
            UIRoomCharacterSelect uICharacterSelect =
                _uiManagerServices.GetSceneUIFromResource<UIRoomCharacterSelect>();
            
            UIRoomChat uiChatting = _uiManagerServices.GetSceneUIFromResource<UIRoomChat>();
            
            _signalBus.TryUnsubscribe<RpcCallerReadySignal>(SpwanCharacterSelect);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<RpcCallerReadySignal>(SpwanCharacterSelect);
        }
    }
}