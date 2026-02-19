using UnityEngine;

namespace Util
{
    public class DebugLogger : MonoBehaviour
    {
        private void OnEnable()
        {
            UtilDebug.Log($"{gameObject.name}가 활성화되었습니다.");
            UtilDebug.Log($"활성화 스택 트레이스:\n{System.Environment.StackTrace}");

        }

        private void OnDisable()
        {
            UtilDebug.Log($"{gameObject.name}가 비활성화되었습니다.");
            UtilDebug.Log($"비활성화 스택 트레이스:\n{System.Environment.StackTrace}");
        }
    }
}