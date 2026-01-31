using System;
using System.Collections.Generic;
using GameManagers;
using GameManagers.RelayManager;
using UnityEngine.SceneManagement;
using Util;
using Zenject;

namespace UI.WorldSpace.PortalIndicator
{
    public class UINgoPortalIndicator : IPortalIndicator
    {
        [Inject] private RelayManager _relayManager;
        private Action _indicatorOffEvent;
        public event Action IndicatorOffEvent
        {
            add => UniqueEventRegister.AddSingleEvent(ref _indicatorOffEvent, value);
            remove => UniqueEventRegister.RemovedEvent(ref _indicatorOffEvent, value);
        }
        public void Initialize()
        {
            //_relayManager.NetworkManagerEx.SceneManager.OnLoadEventCompleted += OnChangeSceneEvent;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnDisableIndicator()
        {
            //_relayManager.NetworkManagerEx.SceneManager.OnLoadEventCompleted -= OnChangeSceneEvent;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
      
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            _indicatorOffEvent?.Invoke();
        }


        #region  12.4일 이벤트 수정
        // 해당 건도 로컬에서만 끄면 되기에 호스트가 아닌 로컬에서 끄도록 설정함
        private void OnChangeSceneEvent(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {

            if (sceneName != Define.Scene.GamePlayScene.ToString() && sceneName != Define.Scene.BattleScene.ToString())
                return;

            if (!clientsCompleted.Contains(_relayManager.NetworkManagerEx.LocalClientId))
                return;

            _indicatorOffEvent?.Invoke();
        }
        

        #endregion
       

    }
}
