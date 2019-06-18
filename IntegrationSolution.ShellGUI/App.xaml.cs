using Integration.Flyouts;
using Integration.Infrastructure;
using Integration.Infrastructure.Views.Account;
using Integration.ModuleGUI;
using IntegrationSolution.Common;
using IntegrationSolution.Dialogs;
using IntegrationSolution.Excel;
using IntegrationSolution.ShellGUI.ControlRegionAdapter;
using IntegrationSolution.ShellGUI.ViewModels;
using log4net;
using MahApps.Metro.Controls;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using WialonBase;

namespace IntegrationSolution.ShellGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Analog of Bootstrapper.
    /// Current implementation of PrismApplication related to Prism 7.
    /// [Why not UnityContainer: it is related to Prism 6]
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            base.InitializeModules();
            this.MainWindow = Container.Resolve<MainWindow>();
            this.MainWindow.DataContext = Container.Resolve<MainWindowViewModel>();
            return this.MainWindow;
        }


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            base.ConfigureServiceLocator();
            log4net.Config.XmlConfigurator.Configure();

            containerRegistry.RegisterSingleton<MainWindowViewModel>();

        }


        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);
            regionAdapterMappings.RegisterMapping(typeof(FlyoutsControl), Container.Resolve<FlyoutsControlRegionAdapter>());
        }


        protected override void OnInitialized()
        {
            base.OnInitialized();
            LogManager.GetLogger(this.GetType()).Info("Запуск программы!");
        }


        /// <summary>
        /// Adding modules which are should be integrated in Installer.Shell
        /// </summary>
        /// <param name="containerProvider"></param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<InfrastructureModule>(); // required module
            moduleCatalog.AddModule<FlyoutsModule>(); // required module
            moduleCatalog.AddModule<DialogsModule>();
            moduleCatalog.AddModule<CommonModule>();
            moduleCatalog.AddModule<ModuleGUIModule>(); // module for views
            moduleCatalog.AddModule<WialonModule>();
            moduleCatalog.AddModule<IntegrationSolutionExcelModule>(); // module with Excel logic
        }
    }
}
