using System.Diagnostics;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Util
{
    public class ParentTrace : MonoBehaviour
    {
        Transform _lastParent;

        void Awake()
        {
            _lastParent = transform.parent;
            UtilDebug.Log($"[ParentTrace/Awake] {FullPath(transform)} parent={FullPath(_lastParent)} local={transform.localScale} lossy={transform.lossyScale}");
        }

        void OnTransformParentChanged()
        {
            var newParent = transform.parent;
            var sb = new StringBuilder();
            sb.AppendLine($"[ParentTrace] {name} parent changed");
            sb.AppendLine($"  from: {FullPath(_lastParent)}");
            sb.AppendLine($"  to  : {FullPath(newParent)}");
            sb.AppendLine($"  local={transform.localScale} lossy={transform.lossyScale}");
            sb.AppendLine(new StackTrace(1, true).ToString()); // 가능하면 호출 스택
            UtilDebug.Log(sb.ToString());
            _lastParent = newParent;
        }

        static string FullPath(Transform t)
        {
            if (t == null) return "(null)";
            var p = t;
            var path = t.name;
            while (p.parent) { p = p.parent; path = p.name + "/" + path; }
            return path;
        }
    }
}