using System;

namespace UI.WorldSpace.PortalIndicator
{
    public interface IPortalIndicator
    {
        public event Action IndicatorOffEvent;
        public void Initialize();
        public void OnDisableIndicator();
    }
}
