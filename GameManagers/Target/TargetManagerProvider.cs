using Scene.CommonInstaller;

namespace GameManagers.Target
{
    /// <summary>
    /// 1.25일 추가
    /// 기존 TargetManager가 씬마다 생성되는데, 씬이 변경될때마다.
    /// 런타임 스킬들이 이전 씬의 TargetManager를 물고 있는걸 확인.
    /// 차라리 ProjectContext를 만들어 씬마다 타겟매니저가 생성이 돼서
    /// 타겟매니저가 해당 프로바이더에게 등록하면, 프로바이더는 타겟매니저를 제공해주는 역할을 함 
    /// </summary>
    public class TargetManagerProvider : IRegistrar<ITargetManager>
    {
        private ITargetManager _targetManager;
        public ITargetManager TargetManager => _targetManager;
        public void Register(ITargetManager targetManager)
        {
            _targetManager = targetManager;
        }

        public void Unregister(ITargetManager sceneContext)
        {
            if (_targetManager != null)
            {
                _targetManager = null;
            }
        }
    }
}
