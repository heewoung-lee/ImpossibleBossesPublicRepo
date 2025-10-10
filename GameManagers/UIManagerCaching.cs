using System;
using System.Collections.Generic;
using GameManagers.Interface;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIManager;
using ProjectContextInstaller;
using Scene.CommonInstaller;
using Scene.ZenjectInstaller;
using UI;
using UI.Popup;
using UI.Popup.PopupUI;
using UI.Scene;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers
{
    internal class UIManagerRequestCaching: IRegistrar<ICachingForUI>,IUIManagerServices
    {
        private ICachingForUI _iCachingForUI;
        private IUIorganizer _organizer;
        private IUIPopupManager _popupManager;
        private IUISceneManager _sceneManager;
        private IUISubItem _subItem;
        private IEnumerable<IRegisterCachingUI> _cachingForUI;
        [Inject]
        public UIManagerRequestCaching(
            IUIorganizer organizer,
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IUIPopupManager popupManager,
            [Inject(Id =ResourcesLoaderInstaller.ResourceBindCode)]
            IUISceneManager sceneManager,
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IUISubItem subItem,
            IEnumerable<IRegisterCachingUI> cachingUIs)
        {
            _organizer = organizer;
            _popupManager = popupManager;
            _sceneManager = sceneManager;
            _subItem = subItem;
            _cachingForUI = cachingUIs;
        }
        
        
        public void Register(ICachingForUI sceneContext)
        {
            _iCachingForUI = sceneContext;
            foreach (IRegisterCachingUI cachingUI in _cachingForUI)
            {
                cachingUI.RegisterCachingUI(_iCachingForUI);
            }
            
        }
        public void Unregister(ICachingForUI sceneContext)
        {
            if (_iCachingForUI == sceneContext)
            {
                _iCachingForUI = null;
                (_organizer as IDisposable)?.Dispose();
                foreach (IRegisterCachingUI cachingUI in _cachingForUI)
                {
                    cachingUI.RegisterCachingUI(null);
                }
            }
        }

        
        public GameObject Root => _organizer.Root;
        
        public void SetCanvas(Canvas canvas, bool sorting = false)//씬 넘어갈때 다초기화 할것
        {
            _organizer.SetCanvas(canvas, sorting);
        }
        
        public void AddImportant_Popup_UI(UIPopup importantUI)
        {
            _popupManager.AddImportant_Popup_UI(importantUI);
        }

        public T GetImportant_Popup_UI<T>() where T : UIPopup
        {
           return _popupManager.GetImportant_Popup_UI<T>();
        }

        public T GetPopupInDict<T>() where T : UIPopup
        {
           return _popupManager.GetPopupInDict<T>();
        }

        public bool TryGetPopupDictAndShowPopup<T>(out T uiPopup) where T : UIPopup
        {
           return _popupManager.TryGetPopupDictAndShowPopup<T>(out uiPopup);
        }

        public void ShowPopupUI(UIPopup popup)
        {
            _popupManager.ShowPopupUI(popup);
        }
        
        public void ClosePopupUI()
        {
            _popupManager.ClosePopupUI();
        }

        public void ClosePopupUI(UIPopup popup)
        {
            _popupManager.ClosePopupUI(popup);
        }

        public void CloseAllPopupUI()
        {
            _popupManager.CloseAllPopupUI();
        }

        public void SwitchPopUpUI(UIPopup popup)
        {
            _popupManager.SwitchPopUpUI(popup);
        }

        public bool GetTopPopUpUI(UIPopup popup)
        {
            return _popupManager.GetTopPopUpUI(popup);
        }
        public T GetPopupUIFromResource<T>(string name = null) where T : UIPopup
        {
            return _popupManager.GetPopupUIFromResource<T>(name);
        }
        
        
        public T Get_Scene_UI<T>() where T : UIScene
        {
            return _sceneManager.Get_Scene_UI<T>();
        }
        public T GetSceneUIFromResource<T>(string name = null, string path = null) where T : UIScene
        {
           return _sceneManager.GetSceneUIFromResource<T>(name, path);
        }

        public UIScene GetSceneUIFromResource(Type type, string name = null, string path = null)
        {
           return _sceneManager.GetSceneUIFromResource(type, name, path);
        }

        public bool Try_Get_Scene_UI<T>(out T uiScene) where T : UIScene
        {
            return _sceneManager.Try_Get_Scene_UI(out uiScene);
        }
        public T GetOrCreateSceneUI<T>(string name = null, string path = null) where T : UIScene
        {
           return _sceneManager.GetOrCreateSceneUI<T>(name, path);
        }


        public T MakeUIWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UIBase
        {
          return _subItem.MakeUIWorldSpaceUI<T>(parent, name);
        }

        public T MakeSubItem<T>(Transform parent = null, string name = null, string path = null) where T : UIBase
        {
           return _subItem.MakeSubItem<T>(parent, name, path);
        }

    }
}
