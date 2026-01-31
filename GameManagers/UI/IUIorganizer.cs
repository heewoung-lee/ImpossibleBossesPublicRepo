using UnityEngine;

namespace GameManagers.Interface.UIManager
{
    public interface IUIorganizer
    {
        public int PopupSorting { get; set; }
        public int SceneSorting { get; set; }
        
        public void SetCanvas(Canvas canvas, bool sorting = false);
        public GameObject Root { get; }
        
        
    }
}