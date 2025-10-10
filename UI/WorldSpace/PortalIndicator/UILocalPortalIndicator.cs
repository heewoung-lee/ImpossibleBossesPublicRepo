using System;
using Scene.CommonInstaller.Interfaces;
using Stats;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.WorldSpace.PortalIndicator
{
    public class UILocalPortalIndicator : IPortalIndicator
    {
        private Action _indicatorOffEvent;
        public event Action IndicatorOffEvent
        {
            add => UniqueEventRegister.AddSingleEvent(ref _indicatorOffEvent, value);
            remove => UniqueEventRegister.RemovedEvent(ref _indicatorOffEvent, value);
        }

        public void Initialize()
        {
            Debug.Log("Initializing PortalIndicator");
        }

        public void OnDisableIndicator()
        {
            Debug.Log("OnDisableIndicator");
        }
    }
}
