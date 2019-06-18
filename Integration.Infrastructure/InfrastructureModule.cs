using Integration.Infrastructure.Constants;
using Integration.Infrastructure.ViewModels;
using Integration.Infrastructure.ViewModels.Account;
using Integration.Infrastructure.ViewModels.Logistics;
using Integration.Infrastructure.Views;
using Integration.Infrastructure.Views.Account;
using Integration.Infrastructure.Views.Logistics;
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
        { }


        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterView<HomeView, HomeViewModel>(nameof(HomeView), new SingletonLifetimeManager());
            RegisterView<LogisticsQuizView, LogisticsQuizViewModel>(nameof(LogisticsQuizView));

            containerRegistry.RegisterSingleton<ConfigurationData>();
        }


        public void RegisterView<TView, TViewModel>(string viewName, LifetimeManager lifetime = null)
            where TView : UserControl
            where TViewModel : BindableBase
        {
            if (string.IsNullOrEmpty(viewName)) throw new ArgumentNullException(nameof(viewName));
            if (!_container.IsRegistered<TViewModel>())
                _container.RegisterType<TViewModel>();

            if (lifetime == null)
                lifetime = new TransientLifetimeManager();

            _container.RegisterType<UserControl, TView>(viewName,
                lifetime,
                new InjectionProperty("DataContext", new ResolvedParameter<TViewModel>()));
        }
    }
}
