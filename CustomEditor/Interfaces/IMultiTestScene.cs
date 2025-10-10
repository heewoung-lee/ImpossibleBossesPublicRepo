using System.Collections.Generic;
using Scene;
using Scene.GamePlayScene;
using UnityEngine;

namespace CustomEditor.Interfaces
{
    public interface IMultiTestScene
    {
        public List<MultiTestPlayerInfo> GetMultiTestPlayers();

        public ScriptableObject GetPlayScenarioSO();
    }
}
