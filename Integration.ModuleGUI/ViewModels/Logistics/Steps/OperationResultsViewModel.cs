using Integration.ModuleGUI.Models;
using Integration.ModuleGUI.Views;
using IntegrationSolution.Common.Helpers;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Entities.Interfaces;
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
        #endregion

        public OperationResultsViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            CanGoBack = true;
            CanGoNext = true;
            this.Title = "Результаты";

            GridConfiguration = new GridConfiguration();
        }
        
        public override bool MoveBack()
        {
            //ModuleData.Vehicles.First().Trips.Count
            return true;
        }
        
        public override async Task<bool> MoveNext()
        {
            return true;
        }
    }
}
