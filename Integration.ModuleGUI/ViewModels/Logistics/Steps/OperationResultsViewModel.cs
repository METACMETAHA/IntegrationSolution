using Integration.ModuleGUI.Models;
using Integration.ModuleGUI.Views;
using IntegrationSolution.Common.Helpers;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Common.ModulesExtension.Implementations;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;

namespace Integration.ModuleGUI.ViewModels
{
    public class OperationResultsViewModel : VMLocalBase
    {
        #region Variables
        private GridConfiguration gridConfiguration;
        public GridConfiguration GridConfiguration
        {
            get { return gridConfiguration; }
            set { SetProperty(ref gridConfiguration, value); }
        }

        private bool _isOpenChildPopup;
        public bool IsOpenChildPopup
        {
            get { return _isOpenChildPopup; }
            set { SetProperty(ref _isOpenChildPopup, value); }
        }

        private ChartsVMBase predictionChartContext;
        public ChartsVMBase PredictionChartContext
        {
            get { return predictionChartContext; }
            set { SetProperty(ref predictionChartContext, value); }
        }
        #endregion

        public OperationResultsViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            CanGoBack = true;
            CanGoNext = true;
            this.Title = "Результаты";

            GridConfiguration = new GridConfiguration();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (ModuleData.SimpleDataForReport == null)
                ModuleData.SimpleDataForReport = new ObservableCollection<IntegratedVehicleInfo>(ModuleData.DetailsDataForReport);

        }

        public override bool MoveBack()
        {
            ModuleData.SimpleDataForReport = null;
            ModuleData.DetailsDataForReport = null;
            return true;
        }
        
        public override async Task<bool> MoveNext()
        {
            return true;
        }
    }
}
