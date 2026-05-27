using System;
using Buffer;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;

namespace GameManagers.BufferManagement
{
    public interface IBufferManager
    {
        void ModifyStat(BaseStats stats, StatType type, float value);
        BufferComponent InitBuff(BaseStats targetStat, float duration, StatEffect effect, string iconPath);
        void RemoveBuffer(BufferComponent buffer);
        void ImmediatelyBuffStart(BaseStats targetStats, StatType type, float value);

        public void ApplyActionToTargetsTotal(Collider[] targets, Action<NetworkObject> createPaticle,
            Action<NetworkObject> invokeBuff);

        public void ApplyActionToTargetsWithParticle(Collider[] targets, Action<NetworkObject> createPaticle);
        public void ApplyActionToTargetsWithBuff(Collider[] targets,StatEffect effect,float duration,string buffIconPath);

    }
}
