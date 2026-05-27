using UnityEngine;

namespace Module.UI_Module
{
    public class ChooseCharacterCameraController : MonoBehaviour
    {
        private Camera[] _playerCameras;
        
        Transform _chooseCameraTr;

        public Transform ChooseCameraTr { get => _chooseCameraTr; }

        private void Awake()
        {
            _chooseCameraTr = transform.Find("SelectCamaraTr");
            int childCount = _chooseCameraTr.childCount;
            _playerCameras = new Camera[childCount];
            for (int i = 0; i < childCount; i++)
            {
                _playerCameras[i] = _chooseCameraTr.GetChild(i).GetComponent<Camera>();
            }
        }

        public Camera AllocatedCamera(int idx)
        {
            Camera characterChoiceCamera = _playerCameras[idx];
            characterChoiceCamera.gameObject.SetActive(true);
            return characterChoiceCamera;
        }
        
        public void ReleaseCamera(int idx)
        {
            if (idx >= 0 && idx < _playerCameras.Length)
            {
                Camera cam = _playerCameras[idx];
                
                // 카메라 비활성화
                cam.gameObject.SetActive(false);
                
                //위치 초기화 (선택한 직업 위치에서 기본 위치로 복귀)
                cam.transform.localPosition = Vector3.zero; 
                
                //렌더 텍스처 연결 해제
                cam.targetTexture = null; 
            }
        }
    }
}
