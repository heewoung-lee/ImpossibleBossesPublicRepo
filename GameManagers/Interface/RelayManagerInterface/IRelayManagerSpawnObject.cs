using GameManagers.Interface.ResourcesManager;
using NetWork.NGO;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.RelayManagerInterface
{
    public interface IRelayManagerSpawnObject
    {
        public void ScheduleSpawnAfterInit(RelayManager relayManager);
    }

}