using UI;
using UnityEngine;

namespace GameManagers.UIManagement
{
    public interface IUISubItem
    {
        public T MakeUIWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UIBase;
        public T MakeSubItem<T>(Transform parent = null, string name = null, string path = null) where T : UIBase;

    }
}
