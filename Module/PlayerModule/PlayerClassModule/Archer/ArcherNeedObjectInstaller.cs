using Character.Attack.Archer;
using Character.Skill.AllofSkills.Acher;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule.Archer
{
    public interface IArcherFactoryMarker{ }
    
    public class ArcherNeedObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //1.1일 수정 기존에 일일히 캐릭터에 쓰는 팩토리들을 바인드해야했는데 이게번거로워서
            //마킹 인터페이스를 달고 팩토리에서는 상속만 해주면 자동으로 바인드해주는식으로 바꿈
            
            Container.Bind(x => x.AllInterfaces())
                .To(x => x.AllNonAbstractClasses().DerivingFrom<IArcherFactoryMarker>())
                .AsSingle();

        }
    }
}
