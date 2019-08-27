using Integration.ModuleGUI.Models;
using Integration.ModuleGUI.Views;
using Integration.PartialViews.ViewModels;
using IntegrationSolution.Common.Helpers;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Common.Models;
using IntegrationSolution.Common.ModulesExtension.Implementations;
using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using LiveCharts.Defaults;
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
        private Dictionary<string, List<DateTimePoint>> PrepareData(IEnumerable<TripSAP> sap, IEnumerable<TripWialon> wialon)
        {
            if (sap == null || !sap.Any() ||
                wialon == null || !wialon.Any())
                return null;

            var datesDictSAP = sap.OrderBy(x => x.DepartureFromGarageDate.Date).ToLookup(x => x.DepartureFromGarageDate.Date);
            var datesDictWln = wialon.OrderBy(x => x.Begin.Date).ToLookup(x => x.Begin.Date);

            Dictionary<string, List<DateTimePoint>> resultedCollection = new Dictionary<string, List<DateTimePoint>>()
            {
                { "SAP", new List<DateTimePoint>() },
                { "Wialon", new List<DateTimePoint>() }
            };
            
            var startDate = (sap.FirstOrDefault()?.DepartureFromGarageDate.Date > wialon.FirstOrDefault()?.Begin.Date) ?
                wialon.FirstOrDefault()?.Begin.Date : sap.FirstOrDefault()?.DepartureFromGarageDate.Date;

            if (startDate == null)
                return resultedCollection;

            var endDate = (sap.LastOrDefault()?.DepartureFromGarageDate.Date < wialon.LastOrDefault()?.Begin.Date) ?
                wialon.LastOrDefault()?.Begin.Date : sap.LastOrDefault()?.DepartureFromGarageDate.Date;

            if (endDate == null)
                endDate = startDate;

            var currDate = startDate.Value;

            for (int si = 0, wi = 0;
                currDate <= endDate;)
            {
                var _sap = datesDictSAP.ElementAtOrDefault(si);
                var _wln = datesDictWln.ElementAtOrDefault(wi);
                
                if (_sap != null && _wln != null && _sap.Any() && _wln.Any() &&
                    _sap.First().DepartureFromGarageDate.ToShortDateString().Equals(_wln.First().Begin.ToShortDateString()) &&
                    _sap.First().DepartureFromGarageDate.ToShortDateString().Equals(currDate.ToShortDateString()))
                {
                    double totalSap = 0;
                    foreach (var item in _sap)
                        totalSap += item.TotalMileage;

                    double totalWln = 0;
                    foreach (var item in _wln)
                        totalWln += item.Mileage;

                    resultedCollection["SAP"].Add(new DateTimePoint() { Value = totalSap, DateTime = currDate });
                    resultedCollection["Wialon"].Add(new DateTimePoint() { Value = totalWln, DateTime = currDate });

                    si++;
                    wi++;
                }
                else if ((_sap != null && _sap.FirstOrDefault()?.DepartureFromGarageDate.Date < _wln?.FirstOrDefault()?.Begin.Date && 
                    _sap.FirstOrDefault()?.DepartureFromGarageDate.Date == currDate.Date) ||
                    (_sap != null && _wln == null))
                {
                    double totalSap = 0;
                    foreach (var item in _sap)
                        totalSap += item.TotalMileage;

                    resultedCollection["SAP"].Add(new DateTimePoint() { Value = totalSap, DateTime = currDate });
                    resultedCollection["Wialon"].Add(new DateTimePoint() { Value = double.NaN, DateTime = currDate });

                    si++;
                }
                else if ((_wln != null && _sap?.FirstOrDefault()?.DepartureFromGarageDate.Date > _wln.FirstOrDefault()?.Begin.Date && 
                    _wln.FirstOrDefault()?.Begin.Date == currDate.Date) ||
                    (_wln != null && _sap == null))
                {
                    double totalWln = 0;
                    foreach (var item in _wln)
                        totalWln += item.Mileage;

                    resultedCollection["SAP"].Add(new DateTimePoint() { Value = double.NaN, DateTime = currDate });
                    resultedCollection["Wialon"].Add(new DateTimePoint() { Value = totalWln, DateTime = currDate });

                    wi++;
                }
                else
                {
                    resultedCollection["SAP"].Add(new DateTimePoint() { Value = double.NaN, DateTime = currDate });
                    resultedCollection["Wialon"].Add(new DateTimePoint() { Value = double.NaN, DateTime = currDate });
                }
                currDate = currDate.AddDays(1);
            }

            
            return resultedCollection;
        }
        #endregion
    }
}
