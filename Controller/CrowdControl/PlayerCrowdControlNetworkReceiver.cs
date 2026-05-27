using GameManagers.RelayManagement;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace Controller.CrowdControl
{
    public class PlayerCrowdControlNetworkReceiver : NetworkBehaviour, ICCReceiver, ISpecialModifier
    {
        protected const int RootSpecialCode = 1;
        protected const int StunSpecialCode = 3;
        private const string RootDebuffIconPath = "Art/UI/BuffIcon/Debuff/RedDragon/DontMoveDebuffIcon";
        private const string StunDebuffIconPath = "Art/UI/BuffIcon/Debuff/StoneGolem/StunIcon";

        private RelayManager _relayManager;
        private bool _isRootLocked;
        private bool _isStunned;

        // Root: movement/rotation blocked. Stun: movement/rotation + action input blocked.
        public bool IsMovementLocked => _isRootLocked || _isStunned;
        public bool IsActionLocked => _isStunned;

        [Inject]
        public void Construct(RelayManager relayManager)
        {
            _relayManager = relayManager;
        }

        public void ApplyCC(CCType ccType, GameObject caster, float duration)
        {
            switch (ccType)
            {
                case CCType.Taunt:
                    return;
                case CCType.Root:
                    _relayManager.NgoRPCCaller.Call_InitBuffer_ServerRpc(
                        new StatEffect(StatType.Special, RootSpecialCode, ccType.ToString()),
                        RootDebuffIconPath,
                        duration,
                        NetworkObjectId);
                    return;
                case CCType.Stun:
                    _relayManager.NgoRPCCaller.Call_InitBuffer_ServerRpc(
                        new StatEffect(StatType.Special, StunSpecialCode, ccType.ToString()),
                        StunDebuffIconPath,
                        duration,
                        NetworkObjectId);
                    return;
            }
        }

        public virtual void ApplyModified(float value)
        {
            if (Mathf.Approximately(value, 0f))
            {
                return;
            }

            bool isApply = value > 0f;
            int specialCode = Mathf.RoundToInt(Mathf.Abs(value));

            switch (specialCode)
            {
                case RootSpecialCode:
                    _isRootLocked = isApply;
                    return;
                case StunSpecialCode:
                    _isStunned = isApply;
                    return;
                default:
                    ApplyClassSpecial(specialCode, isApply);
                    return;
            }
        }

        protected virtual void ApplyClassSpecial(int specialCode, bool isApply)
        {
            UtilDebug.LogError($"Can't find defined special buff code: {specialCode}");
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _isRootLocked = false;
            _isStunned = false;
        }
    }
}
