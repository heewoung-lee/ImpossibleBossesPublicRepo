using GameManagers.SoundManagement;
using UnityEngine;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    [DisallowMultipleComponent]
    public class SoundManagerInstaller : MonoInstaller
    {
        /// <summary>
        /// ProjectContext에서 SoundManager를 전역 싱글 인스턴스로 바인딩한다.
        /// 사운드 재생 요청은 ISoundManagerServices 또는 SoundManager로 주입받아 사용한다.
        /// </summary>
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SoundManager>().AsSingle();
        }
    }
}
