using System.Collections.Generic;
using System;
using UnityEngine;

namespace GameManagers.ResourcesExManagement
{
    public interface IResourcesServices
    {
        public T Load<T>(string key) where T : UnityEngine.Object;
        public T[] LoadAll<T>(string key) where T : UnityEngine.Object;
        public bool TryGetLoad<T>(string key, out T loadItem) where T : UnityEngine.Object;
        public GameObject InstantiateByKey(string key, Transform parent = null);
        public T GetOrAddComponent<T>(GameObject go) where T : Component;
        public Component GetOrAddComponent(Type componentType, GameObject go);
        public void DestroyObject(GameObject go,float delay = 0f);
        public GameObject InstantiatePrefab(GameObject prefabObj, Transform parent = null);
        //3.21일 추가 씬에 등록된 팩토리의 목록들을 전부 가져오는 서비스 함수
        //이걸 통해 풀 오브젝트들을 미리 로드 할 수 있다.
        public IReadOnlyCollection<GameObject> GetRegisteredFactoryPrefabs();
        
    }
}
