using Integration.ModuleGUI.ViewModels;
using Integration.ModuleGUI.Views;
using IntegrationSolution.Common.ModulesExtension.Implementations;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;

namespace Integration.Infrastructure.Constants
{
    using Data = KeyValuePair<ViewModelBase, UserControl>;

    public class ConfigurationData : BindableBase
    {
        protected IUnityContainer _container;

        #region Properties
        public ObservableCollection<Data> Steps { get; set; }


        /// <summary>
        /// Current KeyValuePair, where 
        /// Key = ViewModel, 
        /// Value = View
        /// </summary>
        private Data _selectedVM;
        public Data SelectedVM
        {
            get { return _selectedVM; }
            set
            {
                if (_selectedVM.Equals(value)) return;
                _selectedVM.Key?.OnExit(); // subscribe of events
                value.Key?.OnEnter(); // subscribe on events
                SetProperty(ref _selectedVM, value);
            }
        }
        #endregion


        public ConfigurationData(IUnityContainer container)
        {
            _container = container;

            Initialize();
        }


        private void Initialize()
        {
            Steps = new ObservableCollection<Data>()
            {
                { new Data(_container.Resolve<LoadingFilesViewModel>(), _container.Resolve<LoadingFilesView>()) },
                { new Data(_container.Resolve<OperationsViewModel>(), _container.Resolve<OperationsView>()) },
                //{ new Data(_container.Resolve<OperationResultsViewModel>(), _container.Resolve<OperationResultsView>()) },
                { new Data(_container.Resolve<FinalViewModel>(), _container.Resolve<FinalView>()) }
            };

            SelectedVM = Steps.FirstOrDefault();
        }

    }
}
