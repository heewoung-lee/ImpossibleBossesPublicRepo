using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers.RelayManager;
using Scene.CommonInstaller;
using Unity.Netcode;
using UnityEngine;

namespace GameManagers.Interface.RelayManagerInterface.Implementation
{
    public class LocalTestConnection : IConnectionStrategy
    {
        public UniTask<string> StartHostAsync(NetworkManager networkManager, int maxConnections)
        {
            // 1. Transport 설정을 건드리지 않음 (기본값 127.0.0.1 사용)
            Debug.Log("[Local] 로컬 호스트 모드로 시작합니다. (Relay 미사용)");

            // 2. 바로 호스트 시작
            if (networkManager.StartHost())
            {
                // 로컬은 JoinCode가 없으므로 null 혹은 "LOCAL" 반환
                return UniTask.FromResult("LOCAL_TEST_MODE"); 
            }

            return UniTask.FromResult<string>(null);
        }
    }
}
