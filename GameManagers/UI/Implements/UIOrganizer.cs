using System;
using UI;
using UI.Popup;
using UnityEngine;

namespace GameManagers.Interface.UIManager.Implements
{
    public class UIOrganizer : IUIorganizer,IDisposable
    {
        private const int SceneUISortingDefaultValue = 0;
        private const int PopupUISortingDefaultValue = 20;
    
        private int _currentSceneSorting = SceneUISortingDefaultValue;
        private int _currentPopupSorting = PopupUISortingDefaultValue;

        public int SceneSorting
        {
            get => _currentSceneSorting;
            set => _currentSceneSorting = value;
        }
        public int PopupSorting
        {
            get => _currentPopupSorting;
            set => _currentPopupSorting = value;
        }

        public void SetCanvas(Canvas canvas, bool sorting = false)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            if (sorting)
            {
                if (canvas.GetComponent<UIPopup>() != null)
                {
                    _currentPopupSorting++;
                    canvas.sortingOrder = _currentPopupSorting;
                }
                else
                {
                    _currentSceneSorting++;
                    canvas.sortingOrder = _currentSceneSorting;
                }
                
            }
            else
            {
                canvas.sortingOrder = 0;
            }
        }

        public GameObject Root 
        {
            get
            {
                GameObject go = GameObject.Find("@UI_ROOT");
                if (go == null)
                {
                    go = new GameObject() { name = "@UI_ROOT" };
                }
                return go;
            }
        }
        public void Dispose()
        {
            _currentSceneSorting = SceneUISortingDefaultValue;
            _currentPopupSorting = PopupUISortingDefaultValue;
        }
    }
}
