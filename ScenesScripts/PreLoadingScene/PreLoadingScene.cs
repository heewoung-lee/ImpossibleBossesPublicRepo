using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameManagers.DataManagement;
using GameManagers.SceneManagement;
using Util;
using Zenject;

namespace ScenesScripts.PreLoadingScene
{
    public class PreLoadingScene : BaseScene
    {
        private IDataManagerInitializer _dataManagerInitializer;
        private SceneManagerEx _sceneManagerEx;

        [Inject]
        private void Construct(
            IDataManagerInitializer dataManagerInitializer,
            SceneManagerEx sceneManagerEx)
        {
            _dataManagerInitializer = dataManagerInitializer;
            _sceneManagerEx = sceneManagerEx;
        }

        public override Define.SceneName CurrentSceneName => Define.SceneName.PreLoadingScene;

        protected override void StartInit()
        {
            base.StartInit();
            InitializeDataAndMoveLoginSceneAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        protected override void AwakeInit()
        {
        }

        private async UniTaskVoid InitializeDataAndMoveLoginSceneAsync(CancellationToken cancellationToken)
        {
            try
            {
                // 2026.05.19 - DataManager가 NonLazy 생성 시점에 시작한 데이터 로딩 완료를 기다린다.
                await _dataManagerInitializer.WaitUntilInitializedAsync(cancellationToken);
                await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);
                _sceneManagerEx.LoadScene(Define.SceneName.LoginScene);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                UtilDebug.LogError(e);
            }
        }
    }
}
