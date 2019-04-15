using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WialonBase.Configuration;
using WialonBase.Implementation;
using WialonBase.Interfaces;

namespace WialonBase
{
    public class WialonModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        { }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<WialonConnection>();
            containerRegistry.RegisterSingleton<INavigationOperations, WialonOperations>();
        }
    }
}
