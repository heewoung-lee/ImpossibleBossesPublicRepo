using System;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using Scene.CommonInstaller.Interfaces;
using Stats;
using UnityEngine;
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
                IFactoryController registerableFactory) : base(container, loadService, registerableFactory)
            {
                _requestGO = loadService.Load<GameObject>($"Prefabs/UI/WorldSpace/UIPortalIndicator");
            }
        }

        private IPortalIndicator _portalIndicator;

        [Inject]
        public void Construct(IPortalIndicator portalIndicator)
        {
            _portalIndicator = portalIndicator;
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
            _portalIndicator.IndicatorOffEvent += SetIndicatorOff;
            _portalIndicator.Initialize();
        }


        public void OnDisable()
        {
            _portalIndicator.IndicatorOffEvent -= SetIndicatorOff;
            _portalIndicator.OnDisableIndicator();
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
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}