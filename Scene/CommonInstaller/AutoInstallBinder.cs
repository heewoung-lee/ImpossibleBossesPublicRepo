using System.Collections.Generic;
using System.Linq;
using Scene.CommonInstaller.TestInstaller;
using UnityEngine;
using Util;
using Zenject;

namespace Scene.CommonInstaller
{
    //2.12일 내가 생각하더라도 이건 좀 이상하긴 한데 우선 이거보다 좋은 방법이 없으므로 이렇게 쓴다.
    public interface ITestPreInstaller {} //테스트 환경을 만들기 위한 인스톨러
    public interface ITestPostInstaller {} //최종적으로 단위 테스트를 위해 리바인드를 하기 위한 인스톨러
    
   [DisallowMultipleComponent]
    public class AutoInstallBinder : MonoInstaller
    {
        
        public override void InstallBindings()
        {
            List<ITestPreInstaller> testPreInstallers = new List<ITestPreInstaller>();
            List<ITestPostInstaller> testPostInstallers = new List<ITestPostInstaller>();
            MonoInstaller[] installers = 
                this.GetComponents<MonoInstaller>()
                    .Where(installer => installer != this).ToArray();


            foreach (MonoInstaller installer in installers)
            {
                if (installer is ITestPreInstaller testPreInstaller)
                {
                    testPreInstallers.Add(testPreInstaller);
                    continue;
                }
                if (installer is ITestPostInstaller testPostInstaller)
                {
                    testPostInstallers.Add(testPostInstaller);
                    continue;
                }
                
                Container.Inject(installer);
                installer.InstallBindings();    
            }
            
            foreach (ITestPreInstaller preInstaller in testPreInstallers)
            {
                MonoInstaller installer = preInstaller as MonoInstaller;
                Container.Inject(installer);
                installer.InstallBindings();   
            }
            
            foreach (ITestPostInstaller postInstaller in testPostInstallers)
            {
                MonoInstaller installer = postInstaller as MonoInstaller;
                Container.Inject(installer);
                installer.InstallBindings();   
            }
            
        }
    }
}
