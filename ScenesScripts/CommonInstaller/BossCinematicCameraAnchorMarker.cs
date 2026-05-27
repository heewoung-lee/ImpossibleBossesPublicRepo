using UnityEngine;

namespace ScenesScripts.CommonInstaller
{
    public class BossCinematicCameraAnchorMarker : MonoBehaviour
    {
        [SerializeField] private Transform _lookAtTarget;

        public Transform LookAtTarget => _lookAtTarget;
    }
}
