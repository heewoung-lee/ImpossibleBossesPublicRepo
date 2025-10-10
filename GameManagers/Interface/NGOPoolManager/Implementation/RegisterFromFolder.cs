using System.Collections.Generic;
using GameManagers.Interface.ResourcesManager;
using NetWork.BaseNGO;
using NetWork.NGO.InitializeNGO;
using UnityEngine;

namespace GameManagers.Interface.NGOPoolManager.Implementation
{
    public class RegisterFromFolder : INgoPoolRegister
    {
        private readonly IResourcesServices _resourcesServices;
        private readonly RelayManager _relayManager;

        public RegisterFromFolder(IResourcesServices resourcesServices, RelayManager relayManager)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
        }

        public void ResisterPoolObj()
        {
            foreach ((string, int) poolingPrefabInfo in AutoRegisterFromFolder())
            {
                //경로에 맞게 Root가져올 것
                // GameObject pollingNgoRoot = _instantiate.InstantiateByPath("Prefabs/NGO/NGO_Polling_ROOT");
                NgoPoolRootInitialize ngoPoolRootInitialize = _resourcesServices
                    .InstantiateByKey("Prefabs/NGO/NGO_Pooling_ROOT").GetComponent<NgoPoolRootInitialize>();

                GameObject pollingNgoRoot = ngoPoolRootInitialize.gameObject;

                if (pollingNgoRoot != null)
                {
                    _relayManager.SpawnNetworkObj(pollingNgoRoot);
                }

                if (pollingNgoRoot.TryGetComponent(out NgoPoolRootInitialize initialize))
                {
                    initialize.SetRootObjectName(poolingPrefabInfo.Item1);
                }
            }
        }

        private List<(string, int)> AutoRegisterFromFolder()
        {
            GameObject[] poolableNgoList = _resourcesServices.LoadAll<GameObject>("Prefabs");
            List<(string, int)> poolingObjPath = new List<(string, int)>();
            foreach (GameObject go in poolableNgoList)
            {
                if (go.TryGetComponent(out Poolable poolable) &&
                    go.TryGetComponent(out NgoPoolingInitializeBase poolingObj))
                {
                    poolingObjPath.Add((poolingObj.PoolingNgoPath, poolingObj.PoolingCapacity));
                }
            }

            return poolingObjPath;
        }
    }
}
