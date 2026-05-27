using UnityEngine;

namespace Controller.CrowdControl
{
    public enum CCType
    {
        Taunt,
        Root,
        Stun
    }

    public interface ICCReceiver
    {
        void ApplyCC(CCType ccType, GameObject caster, float duration);
    }
}
