using FleetAudit.Wizard.Constants;
using FleetAudit.Wizard.ViewModels;
using FleetAudit.Wizard.ViewModels.Account;
using FleetAudit.Wizard.ViewModels.Info;
using FleetAudit.Wizard.ViewModels.Logistics;
using FleetAudit.Wizard.Views;
using FleetAudit.Wizard.Views.Account;
using FleetAudit.Wizard.Views.Info;
using FleetAudit.Wizard.Views.Logistics;
using FleetAudit.Common.Helpers;
using NotificationConstructor.Implementations;
using NotificationConstructor.Interfaces;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace FleetAudit.Wizard
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
        {

        }


        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Extension method
            _container.RegisterView<HomeView, HomeViewModel>(nameof(HomeView), new SingletonLifetimeManager());
            _container.RegisterView<LogisticsQuizView, LogisticsQuizViewModel>(nameof(LogisticsQuizView));
            _container.RegisterView<InfoView, InfoViewModel>(nameof(InfoView));

            containerRegistry.RegisterSingleton<ConfigurationData>();
            containerRegistry.RegisterSingleton<INotificationManager, NotificationManager>();
        }
        
    }
}
