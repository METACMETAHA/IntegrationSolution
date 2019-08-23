using Integration.ModuleGUI.Models;
using Integration.ModuleGUI.Views;
using Integration.PartialViews.ViewModels;
using IntegrationSolution.Common.Helpers;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Common.ModulesExtension.Implementations;
using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using Prism.Commands;
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

        private IntegratedVehicleInfo selectedVehicleInfoInMilesChart;
        public IntegratedVehicleInfo SelectedVehicleInfoInMilesChart
        {
            get { return selectedVehicleInfoInMilesChart; }
            set { selectedVehicleInfoInMilesChart = value; }
        }

        #endregion

        public OperationResultsViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            CanGoBack = true;
            CanGoNext = true;
            this.Title = "Результаты";
            //PredictionChartContext = new PredictionChartViewModel();

            OnCarChangedCmd = new DelegateCommand(OnCarChanged);
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
            return true;
        }
        
        public override async Task<bool> MoveNext()
        {
            return true;
        }


        #region Commands
        public DelegateCommand OnCarChangedCmd { get; private set; }
        [STAThreadAttribute]
        protected async void OnCarChanged()
        {    
            if (SelectedVehicleInfoInMilesChart == null)
                return;

            if (ModuleData.DetailsDataForReport != null)
            {
                var sel = SelectedVehicleInfoInMilesChart as IntegratedVehicleInfoDetails;
                if (sel == null)
                    return;

                await Task.Run(() => { 
                    PredictionChartContext = new PredictionChartViewModel(
                        PrepareData(sel.TripsSAP, sel.TripsWialon));
                });
            }
        }
        #endregion


        #region Helpers
        // --------------------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------------------
        // set X-axis for dates. Change filling of collection (by points) for dates: from - to
        // --------------------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------------------
        private Dictionary<string, List<double>> PrepareData(IEnumerable<TripSAP> sap, IEnumerable<TripWialon> wialon)
        {
            if (sap == null || !sap.Any() ||
                wialon == null || !wialon.Any())
                return null;

            sap = sap.OrderBy(x => x.DepartureFromGarageDate.Date);
            wialon = wialon.OrderBy(x => x.Begin.Date);

            Dictionary<string, List<double>> resultedCollection = new Dictionary<string, List<double>>()
            {
                { "SAP", new List<double>() },
                { "Wialon", new List<double>() }
            };

            //int indexSap = sap.Count(), indexWialon = wialon.Count();
            
            for (int si = 0, wi = 0;
                si < indexSap || wi < indexWialon; )
            {
                var _sap = sap.ElementAtOrDefault(si);
                var _wln = wialon.ElementAtOrDefault(wi);

                if (_sap != null && _wln != null &&
                    _sap.DepartureFromGarageDate.ToShortDateString().Equals(_wln.Begin.ToShortDateString()))
                {
                    resultedCollection["SAP"].Add(_sap.TotalMileage);
                    resultedCollection["Wialon"].Add(_wln.Mileage);

                    si++;
                    wi++;
                }
                else if ((_sap != null && _sap.DepartureFromGarageDate.Date < _wln?.Begin.Date) ||
                    (_sap != null && _wln == null))
                {
                    resultedCollection["SAP"].Add(_sap.TotalMileage);
                    resultedCollection["Wialon"].Add(double.NaN);

                    si++;
                }
                else if ((_wln != null && _sap?.DepartureFromGarageDate.Date > _wln.Begin.Date) ||
                    (_wln != null && _sap == null))
                {
                    resultedCollection["SAP"].Add(double.NaN);
                    resultedCollection["Wialon"].Add(_wln.Mileage);
                    
                    wi++;
                }
            }

            return resultedCollection;
        }
        #endregion
    }
}
