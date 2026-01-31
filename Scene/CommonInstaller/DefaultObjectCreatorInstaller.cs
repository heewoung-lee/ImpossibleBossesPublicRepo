using GameManagers.CommonImplements;
using GameManagers.Interface;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using UnityEngine;
using Zenject;

namespace Scene.CommonInstaller
{
    public class DefaultObjectCreatorInstaller : Installer<DefaultObjectCreatorInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IDefaultGameObjectFactory>().To<DefaultGameObjectFactory>().AsSingle().NonLazy();
            //SceneContextFactory를 없애는 방향으로 생각 했었으나 생각을 바꿈
            //애초에 부하가 크지 않는 객체라면 팩토리를 바인드 하는것보다. 기본 씬 컨텍스트가 알아서 생성해주고 주입해주는게 더 간편하고 좋다.
        }
    }
}
