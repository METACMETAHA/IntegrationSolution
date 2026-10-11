using FleetAudit.Wizard.Steps.Models;
using FleetAudit.Domain.Implementations;
using FleetAudit.Domain.Interfaces;
using FleetAudit.Domain.SelfEntities;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace FleetAudit.Wizard.Steps
{
    public class ModuleGUIModule : IModule
    {
        private IUnityContainer _container;

        public ModuleGUIModule(IUnityContainer unityContainer)
        {
            _container = unityContainer;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<CommonModuleData>();

            _container.RegisterType(typeof(ICommonCompareIndicator<>), typeof(CompareIndicator<>));
            _container.RegisterType<IntegratedVehicleInfo>();
            _container.RegisterType<IntegratedVehicleInfoDetails>();
            _container.RegisterType<TripSAP>();
        }
        
    }
}
