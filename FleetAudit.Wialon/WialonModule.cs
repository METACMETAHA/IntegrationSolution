using FleetAudit.Domain.Implementations.Wialon;
using FleetAudit.Domain.Interfaces.Wialon;
using Prism.Ioc;
using Prism.Modularity;
using FleetAudit.Wialon.Configuration;
using FleetAudit.Wialon.Implementation;
using FleetAudit.Wialon.Interfaces;

namespace FleetAudit.Wialon
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
