using System;
using Buffer;
using GameManagers.Interface.BufferManager;
using GameManagers.ResourcesEx;
using Stats.BaseStats;
using UI.Scene.SceneUI;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameManagers
{
    public class BufferManager : IInitializable, IBufferManager
    {
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourcesServices;
        private readonly RelayManager.RelayManager _relayManager;

        [Inject]
        public BufferManager(
            IUIManagerServices uiManagerServices,
            IResourcesServices resourcesServices, RelayManager.RelayManager relayManager)
        {
            _uiManagerServices = uiManagerServices;
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
        }

        private UIBufferBar _uiBufferBar;

        private UIBufferBar UIBufferBar
        {
            get
            {
                if (_uiBufferBar == null)
                    _uiBufferBar = _uiManagerServices.Get_Scene_UI<UIBufferBar>();

                return _uiBufferBar;
            }
        }

        public void ModifyStat(BaseStats stats, StatType type, float value)
        {
            #region 1.31일 로직 수정

            // 기존에는 버프매니저에서 플레이어의 스탯을 책임 졌으나,
            // 이후 플레이어가 특수한 버프를 가질때가 생겨, 아예 책임을 플레이어로 미뤄 버림.
            // int intValue = (int)value;
            //
            // switch (type)
            // {
            //     case StatType.Attack:
            //         stats.Plus_Attack_Ability(intValue);
            //         break;
            //     case StatType.Defence:
            //         stats.Plus_Defence_Abillity(intValue);
            //         break;
            //     case StatType.CurrentHp:
            //         stats.Plus_Current_Hp_Abillity(intValue);
            //         break;
            //     case StatType.MaxHP:
            //         stats.Plus_MaxHp_Abillity(intValue);
            //         break;
            //     case StatType.MoveSpeed:
            //         stats.Plus_MoveSpeed_Abillity(intValue);
            //         break;
            //     // 새로운 스탯이 생기면 여기에 case만 추가
            //     default:
            //         Debug.LogWarning($"[BufferManager] 정의되지 않은 StatType: {type}");
            //         break;
            // }

            #endregion

            if (stats != null)
                stats.ModifyStat(type, value);
        }

        public BufferComponent InitBuff(BaseStats targetStat, float duration, StatEffect effect, string iconPath)
        {
            // 1. 버프 UI 생성
            GameObject bufferGo =
                _resourcesServices.InstantiateByKey("Prefabs/Buffer/Buffer", UIBufferBar.BufferContext);
            BufferComponent buffer = _resourcesServices.GetOrAddComponent<BufferComponent>(bufferGo);

            Sprite icon = null;
            if (string.IsNullOrEmpty(iconPath) == false)
            {
                icon = _resourcesServices.Load<Sprite>(iconPath);
            }

            buffer.InitAndStartBuff(targetStat, duration, effect.statType, effect.value, icon, effect.buffname);
            return buffer;
        }

        public void RemoveBuffer(BufferComponent buffer)
        {
            // 버프 적용 값 삭제 (-value)
            ModifyStat(buffer.TarGetStat, buffer.StatType, -buffer.Value);
            _resourcesServices.DestroyObject(buffer.gameObject);
        }

        public void ImmediatelyBuffStart(BaseStats targetStats, StatType type, float value)
        {
            //즉발 효과 (예: 체력 포션)
            //즉발 효과에 대한 VFX가 있다면 SO의 Excuse에 프리펩 정보 넣기
            ModifyStat(targetStats, type, value);
        }

        public void Initialize()
        {
        }

        public void ApplyActionToTargetsTotal(Collider[] targets, Action<NetworkObject> createPaticle,
            Action<NetworkObject> invokeBuff)
        {
            foreach (Collider playersCollider in targets)
            {
                if (playersCollider.TryGetComponent(out NetworkObject playerNgo))
                {
                    createPaticle.Invoke(playerNgo);
                    invokeBuff.Invoke(playerNgo);
                }
            }
        }

        public void ApplyActionToTargetsWithParticle(Collider[] targets, Action<NetworkObject> createPaticle)
        {
            foreach (Collider playersCollider in targets)
            {
                if (playersCollider.TryGetComponent(out NetworkObject playerNgo))
                {
                    createPaticle.Invoke(playerNgo);
                }
            }
        }

        public void ApplyActionToTargetsWithBuff(Collider[] targets, StatEffect effect, float duration,
            string buffIconPath)
        {
            foreach (Collider playersCollider in targets)
            {
                if (playersCollider.TryGetComponent(out NetworkObject playerNgo))
                {
                    _relayManager.NgoRPCCaller.Call_InitBuffer_ServerRpc(
                        effect,
                        buffIconPath,
                        duration,
                        playerNgo.NetworkObjectId
                    );
                }
            }
        }
    }
}