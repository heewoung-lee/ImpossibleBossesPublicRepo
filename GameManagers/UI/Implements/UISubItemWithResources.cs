using System;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using ProjectContextInstaller;
using UI;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers.Interface.UIManager.Implements
{
    public class UISubItemWithResources : IUISubItem
    {
        private readonly IResourcesServices _resourcesServices;
        
        public UISubItemWithResources(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }
        public T MakeUIWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UIBase
        {
            if (name == null)
                name = typeof(T).Name;


            GameObject go = _resourcesServices.InstantiateByKey($"Prefabs/UI/WorldSpace/{name}");

            if (parent != null)
                go.transform.SetParent(parent);

            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.Log($"Failed to Load Canvas: GameObject Name:{go.name}");
                return null;
            }
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            return go.GetComponent<T>();
        }

        public T MakeSubItem<T>(Transform parent = null, string name = null, string path = null) where T : UIBase
        {
            if (name == null)
                name = typeof(T).Name;

            GameObject go;
            if (path == null)
            {
                go = _resourcesServices.InstantiateByKey($"Prefabs/UI/SubItem/{name}");
            }
            else
            {
                go = _resourcesServices.InstantiateByKey($"{path}");
            }
            if (parent != null)
                go.transform.SetParent(parent);

            return _resourcesServices.GetOrAddComponent<T>(go);
        }

    }
}
