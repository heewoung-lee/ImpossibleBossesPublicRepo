using System.Collections.Generic;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIManager;
using GameManagers.ResourcesEx;
using UI.Popup;
using UI.Popup.PopupUI;
using UnityEngine;

namespace GameManagers.UI.Implements
{
    public class UIPopupManagerWithResources: IUIPopupManager,IRegisterCachingUI
    {
        private ICachingForUI _iCachingForUI;
        private IResourcesServices _resourcesServices;
        private IUIorganizer _uiorganizer;
        public UIPopupManagerWithResources(
            IResourcesServices resourcesServices,IUIorganizer uiorganizer)
        {
            _resourcesServices = resourcesServices;
            _uiorganizer = uiorganizer;
        }
        public void RegisterCachingUI(ICachingForUI icachingForUI)
        {
            _iCachingForUI = icachingForUI;
        }

        public void AddImportant_Popup_UI(UIPopup importantUI)
        {
            _iCachingForUI.OverWritePopupUI(importantUI);
        }

        public T GetImportant_Popup_UI<T>() where T : UIPopup
        {
            if(_iCachingForUI.TryGetPopupUI<T>(out UIPopup value) == true)
            {
                return value as T;
            }
            Debug.Log($"Not Found KeyType: {typeof(T)}");
            return null;
        }
    
        public T GetPopupInDict<T>() where T : UIPopup
        {
            T popup = GetImportant_Popup_UI<T>();
            if (popup == null)
            {
                popup = GetPopupUIFromResource<T>();
                AddImportant_Popup_UI(popup);
            }
            return popup;
        }

        public bool TryGetPopupDictAndShowPopup<T>(out T uiPopup) where T : UIPopup
        {
            uiPopup = GetPopupInDict<T>();
            if (uiPopup != null)
            {
                ShowPopupUI(uiPopup);
                return true;
            }
            return false;
        }

        public T GetPopupUIFromResource<T>(string name = null) where T : UIPopup
        {
            if (name == null)
                name = typeof(T).Name;

            GameObject go = _resourcesServices.InstantiateByKey($"Prefabs/UI/Popup/{name}");
            T popup = _resourcesServices.GetOrAddComponent<T>(go);
            
            go.transform.SetParent(_uiorganizer.Root.transform);

            if(popup is IPopupHandler handler)
            {
                handler.ClosePopup();
            }
            else
            {
                popup.gameObject.SetActive(false);
            }
            return popup;
        }

        public void ShowPopupUI(UIPopup popup)
        {
            IPopupHandler handler = popup as IPopupHandler;

            if (handler != null && handler.IsVisible == true)
                return;
            else if (handler == null && popup.gameObject.activeSelf == true)
                return;
            
            Canvas canvas = _resourcesServices.GetOrAddComponent<Canvas>(popup.gameObject);
            _uiorganizer.SetCanvas(canvas, true);
            _iCachingForUI.PushPopupUI(popup);
            if (handler != null)
            {
                handler.ShowPopup();
            }
            else
            {
                popup.gameObject.SetActive(true);
            }
        }

        public void ClosePopupUI()
        {
            if (_iCachingForUI.GetPopupStackCount()<= 0)
                return;

            UIPopup popup = _iCachingForUI.CloseTopPopupUI();
            if (popup is IPopupHandler handler)
            {
                handler.ClosePopup();
            }
            else
            {
                popup.gameObject.SetActive(false);
            }
            _uiorganizer.PopupSorting--;
        }

        public void ClosePopupUI(UIPopup popup)
        {
            IPopupHandler handler = popup as IPopupHandler;

            if (handler != null && handler.IsVisible == false)
                return;
            else if (handler == null && popup.gameObject.activeSelf == false)
                return;


            Stack<UIPopup> tempUIPopupStack = new Stack<UIPopup>();

            while (_iCachingForUI.GetPopupStackCount()> 0)
            {
                UIPopup popupUI = _iCachingForUI.CloseTopPopupUI();
                if (popupUI == popup)//나와 _ui_Popups에서 꺼낸 popup이 같다면 종료
                {
                    if (handler != null)
                    {
                        handler.ClosePopup();
                    }
                    else
                    {
                        popup.gameObject.SetActive(false);
                    }
                    _uiorganizer.PopupSorting--;
                    break;
                }
                else
                {
                    tempUIPopupStack.Push(popupUI); //아니라면 스택임시보관소에 저장
                }
            }

            while (tempUIPopupStack.Count > 0)//임시로 보관된 팝업창들을 다시 _ui_Popups에 붓는다.
            {
                _iCachingForUI.PushPopupUI(tempUIPopupStack.Pop());
            }
        }

        public void CloseAllPopupUI()
        {
            while (_iCachingForUI.GetPopupStackCount() > 0)
                ClosePopupUI();
        }

        public void SwitchPopUpUI(UIPopup popup)
        {
            if (popup is IPopupHandler handler)
            {
                if (handler.IsVisible == false)
                {
                    ShowPopupUI(popup);
                }
                else
                {
                    ClosePopupUI(popup);
                }
            }
            else
            {
                if (popup.gameObject.activeSelf == false)
                {
                    ShowPopupUI(popup);
                }
                else
                {
                    ClosePopupUI(popup);
                }
            }
        }

        public bool IsTopPopupUI(UIPopup popup)
        {
            if (_iCachingForUI.GetPopupStackCount() <= 0)
                return false;

            if (_iCachingForUI.GetTopPopupUI() == popup)
            {
                return true;
            }
            return false;
        }
        
    }
}
