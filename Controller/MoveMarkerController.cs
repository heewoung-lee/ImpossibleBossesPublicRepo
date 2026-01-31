using CoreScripts;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using GameManagers.UIFactory.UIController;
using NetWork.NGO;
using Player;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Controller
{
    public class MoveMarkerController : ZenjectMonoBehaviour
    {
        public class MoveMarkerControllerFactory : SceneComponentFactory<MoveMarkerController>{}


        private IPlayerSpawnManager _gameManagerEx;
        private IVFXManagerServices _vfxManager;


        [Inject]
        public void Construct(IPlayerSpawnManager gameManagerEx, IVFXManagerServices vfxManager)
        {
            _gameManagerEx = gameManagerEx;
            _vfxManager = vfxManager;
        }


        private void RegiterPlayerMoveMarker(PlayerController controller)
        {
            controller.OnPlayerMouseClickPosition += InstantiateMoveMarker;
        }
        //현재 문제 Controller가 늦게 달림.

        private void UnRegiterPlayerMoveMarker(PlayerController controller)
        {
            controller.OnPlayerMouseClickPosition -= InstantiateMoveMarker;
        }

        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            _gameManagerEx.OnPlayerSpawnwithController -= RegiterPlayerMoveMarker;
            //구독되어있는 마커이벤트를 빼주고,
            GameObject player = _gameManagerEx.GetPlayer();
            if (player != null && player.TryGetComponent(out PlayerController controller) == true)
            {
                UnRegiterPlayerMoveMarker(controller);
            }
            //해당 클래스가 없어지는데 플레이어가 남아있다면. 플레이어에게 등록된 이벤트도 같이 지워준다.
        }


        private void InstantiateMoveMarker(Vector3 markerPosition)
        {
            _vfxManager.InstantiateParticleInArea("Prefabs/Particle/WayPointEffect/Move", markerPosition);
        }

        protected override void ZenjectEnable()
        {
            if (_gameManagerEx.GetPlayer() == null ||
                _gameManagerEx.GetPlayer().GetComponent<PlayerController>() == null)
            {
                _gameManagerEx.OnPlayerSpawnwithController += RegiterPlayerMoveMarker;
            }
            else
            {
                RegiterPlayerMoveMarker(_gameManagerEx.GetPlayer().GetComponent<PlayerController>());
            }
        }
        protected override void InitAfterInject()
        {
          
        }
    }
}