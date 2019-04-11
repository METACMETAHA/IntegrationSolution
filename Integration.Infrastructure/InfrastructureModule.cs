using Integration.Infrastructure.Constants;
using Integration.Infrastructure.ViewModels;
using Integration.Infrastructure.Views;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Integration.Infrastructure
{
    /// <summary>
    /// Current project is main library for Installer.Shell.
    /// This is where all the work of loading child modules is done.
    /// </summary>
    public class InfrastructureModule : IModule
    {
        IUnityContainer _container;


        public InfrastructureModule(IUnityContainer container)
        {
            _container = container;
        }


        public void OnInitialized(IContainerProvider containerProvider)
        { }


        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ConfigurationData>();
        }
    }
}
