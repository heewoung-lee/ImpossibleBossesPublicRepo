using System;
using System.Collections.Generic;
using GameManagers;
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
            _relayManager.NetworkManagerEx.SceneManager.OnLoadEventCompleted += OnChangeSceneEvent;
        }

        public void OnDisableIndicator()
        {
            _relayManager.NetworkManagerEx.SceneManager.OnLoadEventCompleted -= OnChangeSceneEvent;
        }


        private void OnChangeSceneEvent(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {

            if (sceneName != Define.Scene.GamePlayScene.ToString() && sceneName != Define.Scene.BattleScene.ToString())
                return;

            if (!clientsCompleted.Contains(_relayManager.NetworkManagerEx.LocalClientId))
                return;

            _indicatorOffEvent?.Invoke();
        }

    }
}
