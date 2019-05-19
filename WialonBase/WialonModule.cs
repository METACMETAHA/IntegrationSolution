using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces.Wialon;
using Prism.Ioc;
using Prism.Modularity;
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
            containerRegistry.RegisterSingleton<INavigationOperations, WialonWrapper>();

            containerRegistry.Register<ITripWialon, TripWialon>(nameof(TripWialon));
            containerRegistry.Register<ITripWialon, SpeedViolationWialon>(nameof(SpeedViolationWialon));
        }
    }
}
