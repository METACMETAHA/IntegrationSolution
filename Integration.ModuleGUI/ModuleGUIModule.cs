using Integration.ModuleGUI.Models;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace Integration.ModuleGUI
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
