using DialogConstruction.Interfaces;
using IntegrationSolution.Common.Enums;
using IntegrationSolution.Dialogs.ViewModels;
using IntegrationSolution.Dialogs.Views;
using MahApps.Metro.Controls.Dialogs;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace IntegrationSolution.Dialogs
{
    public class DialogsModule : IModule
    {
        private IUnityContainer _container;

        public DialogsModule(IUnityContainer unityContainer)
        {
            _container = unityContainer;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDialogManager, DialogConstruction.Implementations.DialogManager>();   
            
            RegisterDialog<TestDialogView, TestDialogViewModel>(DialogNamesEnum.TestDialog.ToString());
            RegisterDialog<FuelPriceInputDialog, FuelPriceInputDialogVM>(DialogNamesEnum.FuelPriceDialog.ToString());
        }

        public void RegisterDialog<TView, TViewModel>(string dialogName)
            where TView : BaseMetroDialog
            where TViewModel : IDialogViewModel
        {
            if (string.IsNullOrEmpty(dialogName)) throw new ArgumentNullException("dialogName");
            if (!_container.IsRegistered<TViewModel>())
                _container.RegisterType<TViewModel>();

            _container.RegisterType<BaseMetroDialog, TView>(dialogName,
                new PerResolveLifetimeManager(),
                new InjectionProperty("DataContext", new ResolvedParameter<TViewModel>()));
        }
    }
}
