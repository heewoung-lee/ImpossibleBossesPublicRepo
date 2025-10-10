using Scene.GamePlayScene;
using Scene.GamePlayScene.Spawner;
using UnityEngine;

namespace Module.NGO_Module
{
    public class ModuleNgoGamePlaySceneSpawn : MonoBehaviour
    {
        void Start()
        {
            GetComponent<ISceneSpawnBehaviour>().SpawnObj();
        }
    }
}
