using GameManagers.RelayManagement;
using NetWork;
using NetWork.NGO;
using UnityEngine;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    
    public class EventSignalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<RpcCallerReadySignal>();
            
            Container.DeclareSignal<RuntimeSkillFactoryReadySignal>();

            Container.DeclareSignal<UISkillBarReadySignal>();
            
            Container.DeclareSignal<RelayDisconnectedSignal>();

            Container.DeclareSignal<BossAnimationNetworkReadySignal>();
            Container.DeclareSignal<BossSceneOpeningLocalReadySignal>();
            Container.DeclareSignal<BossSceneOpeningStartSignal>();
        }
    }
    public class RpcCallerReadySignal 
    {
        public NgoRPCCaller CallerInstance; // 혹시나해서 RpcCaller넘김 시그널을 구독할때 파라미터를 이걸로 받아도됨
    }
    /// <summary>
    /// 팩토리가 준비되면 신호를 주는 시그널 기존에는 필요가 없었지만,
    /// 타겟팅 스킬에 필요한 TargetManager가 필요해짐에 따라 초기화 타이밍이 밀려 필요해지게됨
    /// </summary>
    public class RuntimeSkillFactoryReadySignal {}
    
   
    
    /// <summary>
    ///  UI스킬바가 준비되었는지 확인하는 시그널 기존 SkillManager가 담당했으나,
    ///  스킬매니저의 담당역할이 축소해야함으로 시그널로 이전함.
    /// </summary>
    public class UISkillBarReadySignal {}



    /// <summary>
    /// 릴레이서버가 끊어졌을때의 이벤트
    /// 로비나 씬매니저가 받아서 플레이어를 게임플레이씬에서 로비씬으로 이동시킴.
    /// </summary>
    public struct RelayDisconnectedSignal
    {
        public ulong DisconnectedId;
        public RelayDisconnectCause Cause;
    }

    public class BossAnimationNetworkReadySignal
    {
        public GameObject BossMonster;
        public BossAnimationNetworkController AnimationController;
    }

    public class BossSceneOpeningLocalReadySignal
    {
    }

    public class BossSceneOpeningStartSignal
    {
        public double StartServerTime;
    }
}
