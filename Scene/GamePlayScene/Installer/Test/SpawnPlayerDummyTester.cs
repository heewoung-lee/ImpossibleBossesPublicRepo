using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using NetWork.NGO.Scene_NGO;
using Scene.GamePlayScene.Spawner;
using Stats.BaseStats;
using UnityEngine;

namespace Scene.GamePlayScene.Installer.Test
{
    public class SpawnPlayerDummyTester : ISceneSpawn
    {
        public void SpawnObject(IResourcesServices resources, RelayManager relayManager)
        {
            if (relayManager.NetworkManagerEx.IsHost == false)
                return;

            BaseStats dummy1 = resources.InstantiateByKey("Prefabs/NPC/PlayerTestDummy").GetComponent<BaseStats>();
            relayManager.SpawnNetworkObj(dummy1.gameObject, relayManager.NgoRoot.transform,
                position: new Vector3(8f, 0, -2.5f));

            BaseStats dummy2 = resources.InstantiateByKey("Prefabs/NPC/PlayerTestDummy").GetComponent<BaseStats>();
            relayManager.SpawnNetworkObj(dummy2.gameObject, relayManager.NgoRoot.transform,
                position: new Vector3(6f, 0, -4.5f));

            BaseStats dummy3 = resources.InstantiateByKey("Prefabs/NPC/PlayerTestDummy").GetComponent<BaseStats>();
            relayManager.SpawnNetworkObj(dummy3.gameObject, relayManager.NgoRoot.transform,
                position: new Vector3(4f, 0, 3.5f));

            BaseStats dummy4 = resources.InstantiateByKey("Prefabs/NPC/PlayerTestDummy").GetComponent<BaseStats>();
            relayManager.SpawnNetworkObj(dummy4.gameObject, relayManager.NgoRoot.transform,
                position: new Vector3(2f, 0, +2.5f));

            BaseStats dummy5 = resources.InstantiateByKey("Prefabs/NPC/PlayerTestDummy").GetComponent<BaseStats>();
            relayManager.SpawnNetworkObj(dummy5.gameObject, relayManager.NgoRoot.transform,
                position: new Vector3(0f, 0, 2.5f));

            BaseStats dummy6 = resources.InstantiateByKey("Prefabs/NPC/PlayerTestDummy").GetComponent<BaseStats>();
            relayManager.SpawnNetworkObj(dummy6.gameObject, relayManager.NgoRoot.transform,
                position: new Vector3(-1f, 0, 0f));


            NgoBossRoomEntrance ngoBossRoomEntrance = resources
                .InstantiateByKey("Prefabs/NGO/Scene_NGO/NGOBossRoomEntrance").GetComponent<NgoBossRoomEntrance>();
            relayManager.SpawnNetworkObj(ngoBossRoomEntrance.gameObject, relayManager.NgoRoot.transform);

            NgoStageTimerController ngoStageTimerController = resources
                .InstantiateByKey("Prefabs/NGO/Scene_NGO/NgoStageTimerController")
                .GetComponent<NgoStageTimerController>();
            relayManager.SpawnNetworkObj(ngoStageTimerController.gameObject, relayManager.NgoRoot.transform);
        }
    }
}
