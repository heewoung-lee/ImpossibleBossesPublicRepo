using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Test.TestScripts
{
    public class TestChangeMeterial : MonoBehaviour
    {
        [SerializeField] private Material testMaterial;
    
        void Start()
        {
            // 1. 내 족보의 가장 꼭대기(최상위 부모)를 먼저 찾는다. (보통 Player가 됨)
            Transform rootObject = transform.root; 

            // 2. 그 꼭대기에서부터 아래에 있는 모든 자손을 싹 다 뒤진다.
            // (이렇게 해야 내 형제, 조카, 삼촌 위치에 있는 렌더러까지 다 찾음)
            var allRenderers = rootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true); // true는 비활성화된 것도 찾겠다는 뜻

            // 3. 적용
            foreach (var renderer in allRenderers)
            {
                // 머티리얼 배열 전체 교체 로직...
                Material[] newMats = new Material[renderer.materials.Length];
                for (int i = 0; i < newMats.Length; i++) newMats[i] = testMaterial;
                renderer.materials = newMats;
            }
        }

    }
}
