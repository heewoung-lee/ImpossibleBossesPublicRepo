using GameManagers.Interface.ResourcesManager;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameManagers.ResourcesEx.implementation
{
    public class ResourcesLoader : IResourcesLoader
    {
        public T Load<T>(string key) where T : Object
        {
            T loadObj = Resources.Load<T>(key);
            Assert.IsNotNull(loadObj,$"loadObj != null key: {key}");
            return loadObj;
        }

        public T[] LoadAll<T>(string key) where T : Object
        {
            T[] loadObjs = Resources.LoadAll<T>(key);
            Assert.IsNotNull(loadObjs, $"loadObjs != null key: {key}");
            return loadObjs; // 이미 로드한 변수를 리턴
        }

        public bool TryGetLoad<T>(string key, out T loadItem) where T : Object
        {
            loadItem = Resources.Load<T>(key);
            //수정 12.22 이전에 Load<T> 를 사용했었는데 잘못 긁어오면
            //Assert에서 바로 에러나버리니 false를 반환할 수 가 없음.
            //Load<T> -> Resource.Load<T>로 변경

            if (loadItem == null)
                return false;
            else
                return true;
        }
    }
}
