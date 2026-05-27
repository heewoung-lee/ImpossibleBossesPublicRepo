using System;
using System.Linq;
using NetWork.NGO;
using UI.Scene.SceneUI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Module.UI_Module
{
    public enum SelectDirection
    {
        LeftClick = -1,
        RightClick = 1
    }


    public class ModuleChooseCharacterMove : MonoBehaviour
    {

        private const int Movevalue = 4;
        private int _currentSelectCharactorIndex = 0;
        private Transform _chooseCameraTr;
        private UICharacterSelectRect _uiCharacterSelectRect;
        private Action<int, int> _onCameraMoveRequested;
        
        private Button _previousButton;
        private Button _nextButton;

        private int _playerChooseIndex;

        public int PlayerChooseIndex => _playerChooseIndex;
        public event Action<int, int> OnCameraMoveRequested
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _onCameraMoveRequested,value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _onCameraMoveRequested,value);
            }
        }        

        private void Awake()
        {
            _uiCharacterSelectRect = GetComponent<UICharacterSelectRect>();
        }

        private void Start()
        {
            _nextButton = _uiCharacterSelectRect.NextButton;
            _previousButton = _uiCharacterSelectRect.PreviousButton;
            
            _nextButton.onClick.AddListener(MoveRightCamera);
            _previousButton.onClick.AddListener(MoveLeftCamera);
        }

        private void MoveRightCamera()=> MoveSelectCamera(SelectDirection.RightClick);
        private void MoveLeftCamera()=> MoveSelectCamera(SelectDirection.LeftClick);
        public void MoveSelectCamera(SelectDirection direction)
        {
            int index = _currentSelectCharactorIndex;
            
            if (direction == SelectDirection.LeftClick)
                _currentSelectCharactorIndex--;
            else
                _currentSelectCharactorIndex++;

            _currentSelectCharactorIndex = Mathf.Clamp(_currentSelectCharactorIndex, 0, 4);
            
            if(index != _currentSelectCharactorIndex)
            {
                _playerChooseIndex = _currentSelectCharactorIndex;
                
                _onCameraMoveRequested?.Invoke((int)direction, Movevalue);
            }
        }
        private void OnDestroy() 
        {
            if (_nextButton != null) _nextButton.onClick.RemoveListener(MoveRightCamera);
            if (_previousButton != null) _previousButton.onClick.RemoveListener(MoveLeftCamera);
        }
        
        
    }
}