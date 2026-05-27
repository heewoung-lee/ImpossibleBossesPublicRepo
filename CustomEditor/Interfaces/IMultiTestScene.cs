using System.Collections.Generic;
using ScenesScripts;
using UnityEngine;

namespace CustomEditor.Interfaces
{
    public interface IMultiTestScene
    {
        public List<MultiTestPlayerInfo> GetMultiTestPlayers();

        public ScriptableObject GetPlayScenarioSO();
    }
}
