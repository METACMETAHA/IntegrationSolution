using Integration.Infrastructure.Constants;
using Integration.Infrastructure.ViewModels;
using Integration.Infrastructure.ViewModels.Account;
using Integration.Infrastructure.ViewModels.Logistics;
using Integration.Infrastructure.Views;
using Integration.Infrastructure.Views.Account;
using Integration.Infrastructure.Views.Logistics;
using IntegrationSolution.Common.Helpers;
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
        {

        }


        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Extension method
            _container.RegisterView<HomeView, HomeViewModel>(nameof(HomeView), new SingletonLifetimeManager());
            _container.RegisterView<LogisticsQuizView, LogisticsQuizViewModel>(nameof(LogisticsQuizView));

            containerRegistry.RegisterSingleton<ConfigurationData>();
            containerRegistry.RegisterSingleton<INotificationManager, NotificationManager>();
        }
        
    }
}
