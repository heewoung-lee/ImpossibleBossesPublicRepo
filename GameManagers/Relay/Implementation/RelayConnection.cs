using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers.RelayManager;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace GameManagers.Interface.RelayManagerInterface.Implementation
{
    public class RelayConnection: IConnectionStrategy
    {
        public async UniTask<string> StartHostAsync(NetworkManager networkManager, int maxConnections)
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                RelayServerData relaydata = AllocationUtils.ToRelayServerData(allocation, "dtls");
                networkManager.GetComponent<UnityTransport>().SetRelayServerData(relaydata);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                Debug.Log($"호출 됐나요 릴레이코드: {joinCode}");
                if (networkManager.StartHost())
                {
                    return joinCode;
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }
    }
}
