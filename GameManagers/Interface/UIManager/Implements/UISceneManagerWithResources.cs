using System;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.SceneUIManager;
using ProjectContextInstaller;
using UI.Scene;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers.Interface.UIManager.Implements
{
    public class UISceneManagerWithResources: IUISceneManager,IRegisterCachingUI
    {
        private ICachingForUI _iCachingForUI;
        private readonly IResourcesServices _resourcesServices;
        private readonly IUIorganizer _uiorganizer;
        public UISceneManagerWithResources(
            IResourcesServices resourcesServices,IUIorganizer uiorganizer)
        {
            _resourcesServices = resourcesServices;
            _uiorganizer = uiorganizer;
        }
        public void RegisterCachingUI(ICachingForUI icachingForUI)
        {
            _iCachingForUI = icachingForUI;
        } 
    
        public T Get_Scene_UI<T>() where T : UIScene
        {
            if (_iCachingForUI.TryGetSceneUI<T>(out UIScene scene) == true)
            {
                return scene as T;
            }
            Debug.LogError($"Not Found KeyType: {typeof(T)}");
            return null;
        }
        public T GetSceneUIFromResource<T>(string name = null, string path = null) where T : UIScene
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;

            GameObject go = null;
            if (string.IsNullOrEmpty(path))
            {
                go = _resourcesServices.InstantiateByKey($"Prefabs/UI/MainUI/{name}");
            }
            else
            {
                go = _resourcesServices.InstantiateByKey($"{path}");
            }
            T scene = _resourcesServices.GetOrAddComponent<T>(go);
            _iCachingForUI.AddSceneUI(scene);
            
            go.transform.SetParent(_uiorganizer.Root.transform);
            return scene;
        }

        public UIScene GetSceneUIFromResource(Type type, string name = null, string path = null)
        {
            if (!typeof(UIScene).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type {type.Name} is not assignable to {nameof(UIScene)}");
            }

            if (string.IsNullOrEmpty(name))
                name = type.Name;

            GameObject go = null;
            if (string.IsNullOrEmpty(path))
            {
                go = _resourcesServices.InstantiateByKey($"Prefabs/UI/MainUI/{name}");
            }
            else
            {
                go = _resourcesServices.InstantiateByKey($"{path}");
            }

            UIScene scene = _resourcesServices.GetOrAddComponent(type, go) as UIScene;
            
            go.transform.SetParent(_uiorganizer.Root.transform);
            return scene;
                
        }
        
        

        public bool Try_Get_Scene_UI<T>(out T uiScene) where T : UIScene
        {
            if (_iCachingForUI?.TryGetSceneUI<T>(out UIScene scene) == true)
            {
                uiScene = scene as T;
                return uiScene is not null;
            }
            uiScene = null;
            return false;
        }


        public T GetOrCreateSceneUI<T>(string name = null, string path = null) where T : UIScene
        {
            if (_iCachingForUI.TryGetSceneUI<T>(out UIScene scene) == true)
            {
                return scene as T;
            }
            return GetSceneUIFromResource<T>(name, path);
        }


    }
}
