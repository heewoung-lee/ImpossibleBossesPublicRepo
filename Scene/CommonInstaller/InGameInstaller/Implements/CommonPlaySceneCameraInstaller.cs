using GameManagers.Target;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Scene.CommonInstaller.InGameInstaller
{
    public class CommonPlaySceneCameraInstaller : Installer<CommonPlaySceneCameraInstaller>   
    {
        public override void InstallBindings()
        {
            Container.Bind<Camera>().FromMethod(GetMainCamera).AsSingle().NonLazy();
            Container.Bind<CinemachineCamera>().FromMethod(GetCinemachineCamera).AsSingle().NonLazy();
            Container.Bind(
                typeof(IInitializable)
            ,typeof(ITickable)
            ).FromMethod(GetTargetManager).AsSingle().NonLazy();
        }
        private Camera GetMainCamera()
        {
            GameObject[] mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");

            foreach (GameObject camera in mainCameras)
            {
                if (camera.TryGetComponent(out Camera mainCamera))
                {
                    return mainCamera;
                }
            }
            Debug.Assert(false,"MainCamera not found Container Instantiate Camera You should Attached Camera to the Hierarchy ");
            return Container.InstantiatePrefabResource("Prefabs/Camera/CinemachineBrainCamera").GetComponent<Camera>();
        }
        private CinemachineCamera GetCinemachineCamera()
        {
            GameObject[] mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (GameObject camera in mainCameras)
            {
                if (camera.TryGetComponent(out CinemachineCamera mainCamera))
                {
                    return mainCamera;
                }
            }
            Debug.Assert(false,"CinemachineCamera not found Container Instantiate Camera You should Attached Camera to the Hierarchy ");
            return Container.InstantiatePrefabResource("Prefabs/Camera/PlayerFollowingCamera").GetComponent<CinemachineCamera>();
            
        }
        private TargetManager GetTargetManager(InjectContext context)
        {
            TargetManager targetManager = GameObject.FindAnyObjectByType<TargetManager>();
            if (targetManager == null)
            {
                targetManager = Container.InstantiatePrefabResource("Prefabs/Camera/TargetManager").GetComponent<TargetManager>();
            }
            return targetManager;
        }
    }
}
