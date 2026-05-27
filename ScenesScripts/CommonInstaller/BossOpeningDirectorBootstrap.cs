using UnityEngine;
using UnityEngine.Playables;

namespace ScenesScripts.CommonInstaller
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayableDirector))]
    public class BossOpeningDirectorBootstrap : MonoBehaviour
    {
        private PlayableDirector _playableDirector;

        private void Awake()
        {
            _playableDirector = GetComponent<PlayableDirector>();
            _playableDirector.playOnAwake = false;
            _playableDirector.time = 0d;
            _playableDirector.Stop();
        }
    }
}
