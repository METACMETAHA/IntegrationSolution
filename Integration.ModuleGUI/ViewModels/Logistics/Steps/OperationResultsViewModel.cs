using Integration.ModuleGUI.Models;
using Integration.ModuleGUI.Views;
using IntegrationSolution.Common.Helpers;
using IntegrationSolution.Entities.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections;
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
        private bool _verticalGridLinesIsVisible;
        public bool VerticalGridLinesIsVisible
        {
            get { return _verticalGridLinesIsVisible; }
            set { SetProperty(ref _verticalGridLinesIsVisible, value); }
        }

        private bool _headerIsVisible;
        public bool HeaderIsVisible
        {
            get { return _headerIsVisible; }
            set { SetProperty(ref _headerIsVisible, value); }
        }

        #endregion

        public OperationResultsViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            CanGoBack = true;
            CanGoNext = true;
            this.Title = "Результаты";
            HeaderIsVisible = true;
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
