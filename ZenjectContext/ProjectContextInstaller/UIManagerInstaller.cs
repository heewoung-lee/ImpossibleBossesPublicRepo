using GameManagers;
using UnityEngine;
using Zenject;

namespace ProjectContextInstaller
{
   [DisallowMultipleComponent]
   public class UIManagerInstaller : MonoInstaller
   {
      public override void InstallBindings()
      {
         //7.14일 수정 UIManager들의 구현체들이 런타임으로 생성되는 ResouceType에게 의존되다 보니,
         //인스톨러를 삭제 후에 ProjectContext에 빼버림 이후 ResourcesManager인스톨러에 편입
      }
   }
}
