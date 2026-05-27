using Unity.Netcode;
using UnityEngine;

namespace GameManagers.NGOPoolManagement
{
    public interface INetworkObjectGetter
    {
        public NetworkObject GetNetworkObject(string prefabPath, Vector3 position, Quaternion rotation);
        
        public NetworkObject GetNetworkObject(string prefabPath);
    }

    //3.21일 추가 네트워크 오브젝트풀의 임계점을 설정하는 인터페이스
    //기존에는 다 차면 확장하는식으로 진행했는데
    //문제가 다찬 상태에서 확장을 하면 클라이언트가 못받는 문제가 생김.
    //그래서 넉넉하게 그리고 네트워크 컨디션 문제가 없도록 수정
    public interface INetworkObjectPoolExpansionStrategy
    {
        public void TryExpand(string prefabPath);
    }
}
