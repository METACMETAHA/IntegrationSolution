using DialogConstruction.Implementations;
using DialogConstruction.Interfaces;
using Integration.ModuleGUI.Models;
using Integration.ModuleGUI.ViewModels;
using Integration.ModuleGUI.Views;
using IntegrationSolution.Dialogs;
using IntegrationSolution.Dialogs.ViewModels;
using IntegrationSolution.Dialogs.Views;
using MahApps.Metro.Controls.Dialogs;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Injection;

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
        }

        
    }
}
