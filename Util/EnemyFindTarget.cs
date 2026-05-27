using System.Collections.Generic;
using Stats;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Util;

public static class EnemyFindTarget
{
    public static bool IsValidPlayerTarget(GameObject targetObject)
    {
        if (targetObject == null || targetObject.TryGetComponent(out NetworkObject networkObject) == false)
        {
            return false;
        }

        return IsValidPlayerTarget(networkObject, out _);
    }

    public static int CollectValidPlayers(NetworkManager networkManager, List<GameObject> candidates)
    {
        if (candidates == null)
        {
            return 0;
        }

        candidates.Clear();

        if (networkManager?.SpawnManager == null)
        {
            UtilDebug.LogError("NetworkManager or SpawnManager is null");
            return 0;
        }

        foreach (NetworkObject spawnedObject in networkManager.SpawnManager.SpawnedObjectsList)
        {
            if (IsValidPlayerTarget(spawnedObject, out PlayerStats playerStats) == false)
            {
                continue;
            }

            candidates.Add(playerStats.gameObject);
        }

        return candidates.Count;
    }
    
    public static GameObject FindRandomPlayer(NetworkManager networkManager)
    {
        List<PlayerStats> candidates = new List<PlayerStats>();

        if (networkManager?.SpawnManager == null)
        {
            UtilDebug.LogError("NetworkManager or SpawnManager is null");
            return null;
        }

        foreach (NetworkObject spawnedObject in networkManager.SpawnManager.SpawnedObjectsList)
        {
            if (IsValidPlayerTarget(spawnedObject, out PlayerStats playerStats) == false)
            {
                continue;
            }

            candidates.Add(playerStats);
        }
        
        if (candidates.Count == 0)
        {
            UtilDebug.LogError("No player stats were found");
            return null;
        }
        int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[randomIndex].gameObject;
    }

    public static GameObject FindNearestPlayer(Collider[] candidates,int hitCount,GameObject owner)
    {
        float closestDistance = float.MaxValue;
        GameObject nearestObj = null;
        for (int i = 0; i < hitCount; i++)
        {
            Collider col = candidates[i];
            if (col == null) continue;


            if (IsValidPlayerTarget(col.GetComponent<NetworkObject>(), out PlayerStats playerStats) == false)
            {
                continue;
            }

            // 가장 가까운 타겟 계산
            float distance = (owner.transform.position - playerStats.transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestObj = playerStats.gameObject;
            }
        }
        return nearestObj;
    }
    
    
    private static bool IsValidPlayerTarget(NetworkObject spawnedObject, out PlayerStats playerStats)
    {
        playerStats = null;

        if (spawnedObject == null)
            return false;

        if (spawnedObject.TryGetComponent(out playerStats) == false)
            return false;

        if (playerStats.IsDead)
            return false;

        if (spawnedObject.TryGetComponent(out ITargetable targetable) && targetable.IsTargetable == false)
            return false;

        return true;
    }
    
}
