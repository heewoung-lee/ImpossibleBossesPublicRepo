using System;
using Controller;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using GameManagers.Target;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{
    //패킷최적화를 위해 byte 상속
    public enum ChangePlayerStateID : byte 
    {
        Idle = 0
    }
    
    /// <summary>
    /// 1.23일 추가 특정 스킬이 특수한 초기화를 요구하는 경우가 있어 추가함
    /// 해당클래스는 클라이언트가 특수한 스킬을 쓸때 초기화를 도와주는 클래스
    /// </summary>
    [DisallowMultipleComponent]
    public class SkillNetworkRouter : NetworkBehaviour,ISkillNetworkRouter
    {
        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;

        [Inject]
        public void Construct(IResourcesServices resourcesServices,RelayManager relayManager)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
        }

        [Rpc(SendTo.Server)]
        public void RequestSpawnChainSkillServerRpc(
            string prefabPath, 
            ulong startNetId, 
            ulong endNetId, 
            Vector3 startOffset, 
            Vector3 endOffset, 
            float duration)
        {
        
            GameObject vfxInstance = _resourcesServices.InstantiateByKey(prefabPath);
            _relayManager.SpawnNetworkObj(vfxInstance); 

            //데이터 주입
            if (vfxInstance.TryGetComponent(out IChainVfxHandler chainInit))
            {
                // 여기서 StartParticleOption 같은 기본 초기화도 필요하면 수행
                // 핵심 데이터 전송 (이 함수가 NetworkVariable에 값을 넣음)
                chainInit.SetChainData(startNetId, endNetId, startOffset, endOffset, duration);
            }
        }

        //요약: 상태만 줄테니깐 니가 알아서 바꿔
        [Rpc(SendTo.Server)]
        public void RequestPlayerChangeStateRpc(ulong targetNetId,ChangePlayerStateID changeStateID)
        {
            if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetNetId, out NetworkObject player))
            {
                return;
            }
            ulong targetClientId = player.OwnerClientId;
            ReceivePlayerRevalRpc(targetNetId,changeStateID,RpcTarget.Single(targetClientId, RpcTargetUse.Temp));
        }

        //
        [Rpc(SendTo.SpecifiedInParams)]
        private void ReceivePlayerRevalRpc(ulong targetNetId, ChangePlayerStateID changeStateID,RpcParams rpcParams = default)
        {

            if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(targetNetId, out NetworkObject player))
            {
                BaseController controller = player.GetComponent<BaseController>();
                if (controller == null)
                {
                    Debug.Assert(false,"controller is null");
                    return;
                }
                switch (changeStateID)
                {
                    case ChangePlayerStateID.Idle:
                        controller.ForceChangeState(controller.BaseIDleState);
                        BaseStats stats = player.GetComponent<BaseStats>();
                        if(stats != null)
                        {
                            stats.Plus_Current_Hp_Abillity(stats.MaxHp);
                            stats.IsDeadValueChangedRpc(false);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeStateID), changeStateID, null);
                }
            }
        }
        

    }
}