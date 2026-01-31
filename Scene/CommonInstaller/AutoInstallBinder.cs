using System.Collections.Generic;
using System.Linq;
using Scene.CommonInstaller.TestInstaller;
using UnityEngine;
using Zenject;

namespace Scene.CommonInstaller
{
    public interface ITestInstaller {}
    
    
   [DisallowMultipleComponent]
    public class AutoInstallBinder : MonoInstaller
    {
        
        public override void InstallBindings()
        {
            List<ITestInstaller> testInstallers = new List<ITestInstaller>();
            
            MonoInstaller[] installers = 
                this.GetComponents<MonoInstaller>()
                    .Where(installer => installer != this).ToArray();


            foreach (MonoInstaller installer in installers)
            {
                if (installer is ITestInstaller testInstaller)
                {
                    testInstallers.Add(testInstaller);
                    continue;
                }
                Container.Inject(installer);
                installer.InstallBindings();    
            }

            foreach (ITestInstaller testInstaller in testInstallers)
            {
                MonoInstaller installer = testInstaller as MonoInstaller;
                Container.Inject(installer);
                installer.InstallBindings();   
            }
        }
    }
}
