using UnityEngine;
using Util;

namespace Test.TestUI
{
    public class SceneCtxDebug : MonoBehaviour
    {
        void Awake()
        {
            UtilDebug.Log($"[SceneCtxDebug] Awake  →  {gameObject.scene.name}");
        }

        void OnDestroy()
        {
            UtilDebug.Log($"[SceneCtxDebug] Destroy →  {gameObject.scene.name}");
        }
    }
}