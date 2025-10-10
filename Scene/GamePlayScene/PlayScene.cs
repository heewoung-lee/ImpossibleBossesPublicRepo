using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CustomEditor.Multiplay;
using GameManagers;
using NetWork;
using NetWork.NGO;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Sirenix.OdinInspector;
using UI.Scene.SceneUI;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace Scene.GamePlayScene
{
    public class PlayScene : BaseScene, ISkillInit, IHasSceneMover
    {
        private ISceneConnectOnline _sceneConnectOnline;
        private ISceneStarter _gameplaySceneStarter;
        private ISceneMover _sceneMover;


        [Inject]
        public void Construct(
            ISceneConnectOnline sceneConnectOnline,
            ISceneStarter gameplaySceneStarter,
            ISceneMover sceneMover)
        {
            _sceneConnectOnline = sceneConnectOnline;
            _gameplaySceneStarter = gameplaySceneStarter;
            _sceneMover = sceneMover;
        }

        public ISceneMover SceneMover => _sceneMover;


        public override Define.Scene CurrentScene => Define.Scene.GamePlayScene;
        private UILoading _uiLoadingScene;
        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;

        protected override void StartInit()
        {
            base.StartInit();
            _ = StartInitAsync();
        }

        private async Task StartInitAsync()
        {
            await _sceneConnectOnline.SceneConnectOnlineStart();
            try
            {
                _gameplaySceneStarter.SceneStart();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RoomPlayScene] 초기화 중 예외: {e}");
            }
        }

        
    }
}