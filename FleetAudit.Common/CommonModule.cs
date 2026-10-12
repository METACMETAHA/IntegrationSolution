using FleetAudit.Common.Entities;
using FleetAudit.Common.Implementations;
using FleetAudit.Common.Interfaces;
using FleetAudit.Common.Models;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetAudit.Common
{
    public class CommonModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        { }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<FuelPrice>();
            containerRegistry.RegisterSingleton<AppConfiguration>();
            containerRegistry.RegisterSingleton<SerializeConfigDTO>();
        }
    }
}
