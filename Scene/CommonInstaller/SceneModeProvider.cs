using System;
using GameManagers;
using GameManagers.Scene;
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
        private ISceneMultiMode _sceneMultiMode;

        public SceneModeProvider(ISceneMultiMode sceneMultiMode)
        {
            _sceneMultiMode = sceneMultiMode;
        }
        
        private SceneMode _currentSceneMode;

        SceneMode ISceneProvider.CurrentSceneMode
        {
            get
            {
                if (SceneManagerEx.IsCurrentBootNormal) return SceneMode.NormalBoot;

                if (_sceneMultiMode.GetMultiTestMode() == MultiMode.Solo)
                    return SceneMode.MultiTest_Solo;

                if (_sceneMultiMode.GetMultiTestMode() == MultiMode.Multi)
                    return SceneMode.MultiTest_Multi;
                
                throw new InvalidOperationException("Unknown scene mode combination.");
            }
            
            
        }
    }
}
