using System;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using GameManagers.ResourcesEx;
using Scene.CommonInstaller.Interfaces;
using Stats;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace UI.WorldSpace.PortalIndicator
{
    public class UIPortalIndicator : UIBase
    {
        public class UIPortalIndicatorFactory : GameObjectContextFactory<UIPortalIndicator>
        {
            [Inject]
            public UIPortalIndicatorFactory(DiContainer container, IResourcesServices loadService,
                IFactoryManager factoryManager) : base(container, factoryManager)
            {
                _requestGO = loadService.Load<GameObject>($"Prefabs/UI/WorldSpace/UIPortalIndicator");
            }
        }

        enum Images
        {
            PortalIndicatorImg
        }

        private Image _indicatorImg;

        public void SetIndicatorOff()
        {
            _indicatorImg.gameObject.SetActive(false);
        }

        public void SetIndicatorOn()
        {
            _indicatorImg.gameObject.SetActive(true);
        }

        protected override void AwakeInit()
        {
            Bind<Image>(typeof(Images));
            _indicatorImg = Get<Image>((int)Images.PortalIndicatorImg);
        }
        
        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            SetIndicatorOff();
        }

        private Vector3 SetPosition()
        {
            Transform parentTr = GetComponentInParent<PlayerStats>().transform;
            return parentTr.position + (Vector3.up * 1.5f);
        }

        protected override void StartInit()
        {
            transform.position = SetPosition();
            _indicatorImg.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (Camera.main == null) return;
            
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}