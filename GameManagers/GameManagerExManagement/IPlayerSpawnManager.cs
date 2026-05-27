using System;
using Controller;
using Stats;
using UnityEngine;

namespace GameManagers.GameManagerExManagement
{
    public interface IPlayerSpawnManager
    {
        public void SetPlayer(GameObject player);
        public GameObject GetPlayer();
        public PlayerStats GetPlayerStats();
        public event Action<PlayerStats> OnPlayerSpawnEvent;
        public event Action<PlayerController> OnPlayerSpawnwithController;
        public void InvokePlayerSpawnWithController(PlayerController controller);
    }
}
