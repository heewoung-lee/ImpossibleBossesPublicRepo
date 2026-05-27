using NetWork.NGO;
using UnityEngine;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    public class NgoZenjectHandlerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindFactory<DiContainer,GameObject, NgoZenjectHandler,NgoZenjectHandler.NgoZenjectHandlerFactory>();
        }
    }
}
