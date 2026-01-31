using System;
using Buffer;
using GameManagers;
using GameManagers.Interface.BufferManager;
using GameManagers.Scene;
using GameManagers.UIFactory.SceneUI;
using UI.Scene.Interface;
using UnityEngine;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UIBufferBar : UIScene,ISceneChangeBehaviour
    {
        public class UIBufferBarFactory : SceneUIFactory<UIBufferBar>{}

        [Inject] private IBufferManager _bufferManager;
        [Inject] private SceneManagerEx _sceneManagerEx;
        enum BufferContextTr
        {
            BufferContext
        }


        private Transform _bufferContext;

        public Transform BufferContext { get => _bufferContext; }//여기에 다른애들이 추가를 한다면, 


        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _sceneManagerEx.OnBeforeSceneUnloadLocalEvent += OnBeforeSceneUnload;
        }

        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            _sceneManagerEx.OnBeforeSceneUnloadLocalEvent -= OnBeforeSceneUnload;
        }
        protected override void StartInit()
        {
            Bind<Transform>(typeof(BufferContextTr));
            _bufferContext = Get<Transform>((int)BufferContextTr.BufferContext);
        }

        public void OnBeforeSceneUnload()
        {
            foreach(BufferComponent buffer in _bufferContext.GetComponentsInChildren(typeof(BufferComponent)))
            {
                _bufferManager.RemoveBuffer(buffer);
            }
        }
    }
}
