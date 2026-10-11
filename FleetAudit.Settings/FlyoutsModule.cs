using FleetAudit.Settings.Interfaces;
using FleetAudit.Settings.ViewModels;
using FleetAudit.Settings.Views;
using FleetAudit.Common.Helpers;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;

namespace FleetAudit.Settings
{
    [Module(ModuleName = nameof(FlyoutsModule), OnDemand = true)]
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
