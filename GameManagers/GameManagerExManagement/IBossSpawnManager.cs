using System;
using UnityEngine;

namespace GameManagers.GameManagerExManagement
{
    public interface IBossSpawnManager
    {
        public GameObject GetBossMonster();
        public void SetBossMonster(GameObject bossMonster);
        public event Action OnBossSpawnEvent;
    }
}


