using UnityEngine;

namespace Util
{
    public class UnexpectedComponent : MonoBehaviour
    {
        private void OnEnable()
        {
            UtilDebug.Log($"[DEBUG] {this.GetType().Name} added to {gameObject.name}\n{StackTraceUtility.ExtractStackTrace()}");
        }
    }
}
