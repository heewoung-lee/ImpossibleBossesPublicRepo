using Unity.Netcode;
using Module.PlayerModule;
using Stats;
using Random = UnityEngine.Random;

namespace NetWork.Item
{
    public class CoinAmountNetwork : NetworkBehaviour
    {
        private const int DefaultCoinAmount = 1;
        private const int MinimumCoinAmount = 1;
        private const int MaximumCoinAmountExclusive = 6;

        private readonly NetworkVariable<int> _coinAmount = new NetworkVariable<int>(
            DefaultCoinAmount,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public int CoinAmount => _coinAmount.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer == false)
                return;

            _coinAmount.Value = Random.Range(MinimumCoinAmount, MaximumCoinAmountExclusive);
        }

        public void ApplyPickupReward(ModulePlayerInteraction player)
        {
            PlayerStats playerStats = player.GetComponentInParent<PlayerStats>();
            playerStats.Gold += _coinAmount.Value;
        }
    }
}
