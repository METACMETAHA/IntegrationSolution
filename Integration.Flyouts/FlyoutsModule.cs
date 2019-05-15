using Integration.Flyouts.Interfaces;
using Integration.Flyouts.ViewModels;
using Integration.Flyouts.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Integration.Flyouts
{
    public class FlyoutsModule : IModule
    {
        private readonly IUnityContainer _container;

        public FlyoutsModule(IUnityContainer container)
        {
            _container = container;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        { }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<SettingsViewModel>();

            var regions = _container.Resolve<IRegionManager>();
            regions.RegisterViewWithRegion("FlyoutRegion", typeof(SettingsView));
        }
    }
}
