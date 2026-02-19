using System;
using System.Text;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Data.DataType.ItemType.Interface;
using UI.SubItem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Util
{
    public static class Extension
    {
        public static bool TryGetComponentInParents<T>(this Transform searchPosition,out T findObject) where T : Component
        {
            Transform tr = searchPosition;

            while(tr != null)
            {
                if (tr.TryGetComponent(out T component))
                {
                    findObject = component;
                    return true;
                }
                else
                {
                    tr = tr.parent;
                }
            }
            UtilDebug.Log("Can't Find Object");
            findObject = null;
            return false;
        }


        public static bool TryGetComponentInChildren<T>(this Transform searchPosition, out T findObject)
            where T : Component
        {
            findObject = searchPosition.GetComponentInChildren<T>();
            
            if(findObject == null) return false;
            
            return true;
        }

        public static GameObject RemoveCloneText(this GameObject go)
        {
            int index = go.name.IndexOf("(Clone)");
            if (index > 0)
                go.name = go.name.Substring(0, index);

            return go;
        }
        public static GameObject RemoveFactoryText(this GameObject go)
        {
            int index = go.name.IndexOf("(Factory)");
            if (index > 0)
                go.name = go.name.Substring(0, index);

            return go;
        }
        public static GameObject ReplaceWithText(this GameObject go,StringBuilder addtext)
        {
            StringBuilder sb = new StringBuilder();
            int index = go.name.IndexOf("(Clone)");
            if (index > 0)
            {
                sb.Append(go.name, 0, index);
            }
            else
            {
                sb.Append(go.name);
            }
            sb.Append(addtext);
            go.name = sb.ToString();
            return go;
        }
        
    }
}