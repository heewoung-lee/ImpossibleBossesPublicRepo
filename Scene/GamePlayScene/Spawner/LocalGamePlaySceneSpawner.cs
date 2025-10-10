using GameManagers;
using GameManagers.Interface.ResourcesManager;
using NetWork.NGO.InitializeNGO;
using NetWork.NGO.Scene_NGO;
using NPC.Dummy;
using UnityEngine;
using Zenject;

namespace Scene.GamePlayScene.Spwaner
{
    public class LocalGamePlaySceneSpawner : MonoBehaviour
    {
        private IResourcesServices _resourcesServices;
        
        [Inject]
        public void Construct(IResourcesServices resourcesServices, RelayManager relayManager)
        {
            _resourcesServices = resourcesServices;
        }


        public void Start()
        {
            SpawnObject();
        }


        public void SpawnObject()
        {
            Dummy dummy = _resourcesServices.InstantiateByKey("Prefabs/NPC/DamageTestDummy").GetComponent<Dummy>();

            dummy.transform.position = new Vector3(10f, 0, -2.5f);

            NgoBossRoomEntrance ngoBossRoomEntrance = _resourcesServices.InstantiateByKey("Prefabs/NGO/Scene_NGO/NGOBossRoomEntrance").GetComponent<NgoBossRoomEntrance>();

            NgoStageTimerController ngoStageTimerController =_resourcesServices.InstantiateByKey("Prefabs/NGO/Scene_NGO/NgoStageTimerController").GetComponent<NgoStageTimerController>();
        }
    }
}
