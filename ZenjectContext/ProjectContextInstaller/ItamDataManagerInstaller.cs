using GameManagers;
using GameManagers.ItamData;
using GameManagers.ItamDataManager;
using GameManagers.UIFactory.SubItemUI;
using UnityEngine;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
   [DisallowMultipleComponent]
   public class ItamDataManagerInstaller : MonoInstaller
   {
      public override void InstallBindings()
      {
          //Container.BindInterfacesTo<ItemDataManager>().AsSingle();
          //12.19일 아이템 매니저 수정

          Container.BindInterfacesTo<ItemDataManager>().AsSingle();
          
          Container.BindInterfacesTo<ItemGradeResourceManager>().AsSingle();
          
          Container.BindInterfacesAndSelfTo<LootItemFactory>().AsSingle();

          Container.Bind<IUIItemFactory>().To<UIItemFactory>().AsSingle();
          //12.21일 추가. UI의 아이템 컴포넌트 팩토리
          
      }
   }
}
