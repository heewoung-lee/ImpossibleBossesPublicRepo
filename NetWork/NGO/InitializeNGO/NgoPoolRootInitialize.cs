using System.IO;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Pool;
using GameManagers.ResourcesEx;
using NetWork.BaseNGO;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.InitializeNGO
{
    public class NgoPoolRootInitialize : NetworkBehaviour
    {
        public class NgoPoolRootInitializeFactory : NgoZenjectFactory<NgoPoolRootInitialize>
        {
            public NgoPoolRootInitializeFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NGO_Pooling_ROOT");
            }
        }


        private IResourcesServices _resourcesServices;
        private NgoPoolManager _poolManager;

        [Inject]
        public void Construct(IResourcesServices resourcesServices, NgoPoolManager poolManager)
        {
            _resourcesServices = resourcesServices;
            _poolManager = poolManager;
        }

        NetworkVariable<FixedString64Bytes> _rootName = new NetworkVariable<FixedString64Bytes>
            ("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        NetworkVariable<FixedString128Bytes> _poolingNgoPath = new NetworkVariable<FixedString128Bytes>
            ("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                transform.SetParent(_poolManager.GetNgoPoolTransform());
            }

            _rootName.OnValueChanged -= OnChangedRootname;
            _rootName.OnValueChanged += OnChangedRootname;

            _poolingNgoPath.OnValueChanged -= OnChangedPoolingNgoPath;
            _poolingNgoPath.OnValueChanged += OnChangedPoolingNgoPath;

            if (IsHost == false) //클라이언트는 바꿨다는 콜백을 못받을 수 있으니 수동으로 확인
            {
                string objectName = gameObject.name;
                string newName = _rootName.Value.ToString();

                //두 값이 다르면 -> 즉 이미 서버가 _rootName을 설정해둔 상황
                if (objectName != newName && string.IsNullOrEmpty(newName) == false)
                {
                    gameObject.name = newName;
                    GeneratePoolObj(_poolingNgoPath.Value.ToString());
                }
            }
        }

        private void OnChangedPoolingNgoPath(FixedString128Bytes previousValue, FixedString128Bytes newValue)
        {
            GeneratePoolObj(newValue.ToString());
        }

        private void OnChangedRootname(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            gameObject.name = newValue.ToString();
        }

        private void GeneratePoolObj(string path)
        {
            _poolManager.NGO_Pool_RegisterPrefab(path,this);
        }

        public void SetRootObjectName(string poolingNgoPath)
        {
            _poolingNgoPath.Value = poolingNgoPath;
            string pathName = Path.GetFileNameWithoutExtension(poolingNgoPath);//순수파일이름 추출
            pathName += "_Root";
            _rootName.Value = pathName;
        }
    }
}