using UnityEngine;
using Util;

namespace ScenesScripts
{
    public class TestSceneName : BaseScene
    {
        private GameObject _player;

        public override Define.SceneName CurrentSceneName => Define.SceneName.Unknown;

        protected override void StartInit()
        {
            base.StartInit();
        }


        protected override void AwakeInit()
        {
           
        }
    }
}