using System;
using GameManagers;
using NUnit.Framework.Constraints;
using Scene.CommonInstaller.Interfaces;

namespace Scene.CommonInstaller
{
    public enum SceneMode
    {
        NormalBoot,
        LocalTest,
        MultiTest_Solo,
        MultiTest_Multi
    }
    
    public class SceneModeProvider: ISceneProvider
    {
        private ISceneTestMode _sceneTestMode;
        private ISceneMultiMode _sceneMultiMode;

        public SceneModeProvider(ISceneTestMode sceneTestMode, ISceneMultiMode sceneMultiMode)
        {
            _sceneTestMode = sceneTestMode;
            _sceneMultiMode = sceneMultiMode;
        }
        
        private SceneMode _currentSceneMode;

        SceneMode ISceneProvider.CurrentSceneMode
        {
            get
            {
                if (SceneManagerEx.IsNormalBoot) return SceneMode.NormalBoot;

                if (_sceneTestMode.GetTestMode() == TestMode.Local)
                    return SceneMode.LocalTest;

                if (_sceneMultiMode.GetMultiTestMode() == MultiMode.Solo)
                    return SceneMode.MultiTest_Solo;

                if (_sceneMultiMode.GetMultiTestMode() == MultiMode.Multi)
                    return SceneMode.MultiTest_Multi;
                
                throw new InvalidOperationException("Unknown scene mode combination.");
            }
            
            
        }
    }
}
