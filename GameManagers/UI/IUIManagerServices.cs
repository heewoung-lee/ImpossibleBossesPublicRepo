using System;
using UI;
using UI.Popup;
using UI.Scene;
using UnityEngine;

namespace GameManagers
{
    public interface IUIManagerServices
    {
        public GameObject Root { get; }
        public void AddImportant_Popup_UI(UIPopup importantUI);
        public T GetImportant_Popup_UI<T>() where T : UIPopup;
        public T Get_Scene_UI<T>() where T : UIScene;
        public bool Try_Get_Scene_UI<T>(out T uiScene) where T : UIScene;
        public void SetCanvas(Canvas canvas, bool sorting = false);
        public T GetPopupInDict<T>() where T : UIPopup;
        public bool TryGetPopupDictAndShowPopup<T>(out T uiPopup) where T : UIPopup;
        public T GetPopupUIFromResource<T>(string name = null) where T : UIPopup;
        public T GetSceneUIFromResource<T>(string name = null, string path = null) where T : UIScene;
        public UIScene GetSceneUIFromResource(Type type, string name = null, string path = null);
        public T GetOrCreateSceneUI<T>(string name = null, string path = null) where T : UIScene;
        public T MakeUIWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UIBase;
        public T MakeSubItem<T>(Transform parent = null, string name = null, string path = null) where T : UIBase;
        public void ShowPopupUI(UIPopup popup);
        public void ClosePopupUI();
        public void ClosePopupUI(UIPopup popup);
        public void CloseAllPopupUI();
        public void SwitchPopUpUI(UIPopup popup);
        public bool IsTopPopupUI(UIPopup popup);
    }
}
