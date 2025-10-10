using GameManagers;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NetWork
{
    public static class ReadySender
    {
        public static void SendClientReady(ulong playerNgoId)
        {
            if (NetworkManager.Singleton.IsHost == true)
                return;
            
           Debug.Log(playerNgoId+"번 User 초기화 완료");
            
            // writer 용량은 여유 있게(예: 32바이트). 부족하면 OverflowException.
            using var writer = new FastBufferWriter(32, Allocator.Temp);
            writer.WriteValueSafe(playerNgoId);

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                "ClientReady",
                NetworkManager.ServerClientId,      // 서버에게 보냄
                writer,
                NetworkDelivery.Reliable            // 신뢰성/순서 보장 QoS
            );
        }
    }
}