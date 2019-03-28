using Integration.ModuleGUI.ViewModels;
using Integration.ModuleGUI.Views;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Integration.ModuleGUI
{
    public class ModuleGUIModule : IModule
    {
        private IUnityContainer _unityContainer;

        public ModuleGUIModule(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LoadingFilesView, LoadingFilesViewModel>();
            containerRegistry.RegisterForNavigation<FinalView, FinalViewModel>();
        }
    }
}
