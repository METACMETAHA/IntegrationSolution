using Integration.Infrastructure;
using Integration.ModuleGUI;
using IntegrationSolution.Excel;
using IntegrationSolution.ShellGUI.ViewModels;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System.Windows;
using Unity;

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
            this.InitializeModules();
            return Container.Resolve<MainWindow>();
        }


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            base.ConfigureServiceLocator();
            
            containerRegistry.RegisterSingleton<MainWindowViewModel>();
        }


        /// <summary>
        /// Adding modules which are should be integrated in Installer.Shell
        /// </summary>
        /// <param name="containerProvider"></param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<InfrastructureModule>(); // required module
            moduleCatalog.AddModule<ModuleGUIModule>(); // module for views
            moduleCatalog.AddModule<IntegrationSolutionExcelModule>(); // module with Excel logic
        }
    }
}
