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
using MahApps.Metro.Controls;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        private ChartsVMBase predictionChartContext;
        public ChartsVMBase PredictionChartContext
        {
            get { return predictionChartContext; }
            set { SetProperty(ref predictionChartContext, value); }
        }

        private ChartsVMBase speedChartContext;
        public ChartsVMBase SpeedChartContext
        {
            get { return speedChartContext; }
            set { SetProperty(ref speedChartContext, value); }
        }

        private IntegratedVehicleInfo selectedVehicleInfoInMilesChart;
        public IntegratedVehicleInfo SelectedVehicleInfoInMilesChart
        {
            get { return selectedVehicleInfoInMilesChart; }
            set { SetProperty(ref selectedVehicleInfoInMilesChart, value); }
        }

        private SpeedViolationWialon selectedSpeedViolationInfoForChart;
        public SpeedViolationWialon SelectedSpeedViolationInfoForChart
        {
            get { return selectedSpeedViolationInfoForChart; }
            set
            {
                if (value == selectedSpeedViolationInfoForChart)
                    return;

                SetProperty(ref selectedSpeedViolationInfoForChart, value);

                if (value != null)
                    SpeedChartContext = new GaugeSpeedChartViewModel(current: value.MaxSpeed, limit: value.SpeedLimit);
            }
        }

        private string searchField;
        public string SearchField
        {
            get { return searchField; }
            set
            {
                if (searchField == value)
                    return;
                
                SetProperty(ref searchField, value);
                RaisePropertyChanged(nameof(VehicleFilteredList));
            }
        }

        private bool isExpanderWithCarsVisible;
        public bool IsExpanderWithCarsVisible
        {
            get { return isExpanderWithCarsVisible; }
            set { SetProperty(ref isExpanderWithCarsVisible, value); }
        }

        private bool isHideNullMileageCars;
        public bool IsHideNullMileageCars
        {
            get { return isHideNullMileageCars; }
            set { SetProperty(ref isHideNullMileageCars, value); }
        }

        private bool isShowOnlyViolationsCars;
        public bool IsShowOnlyViolationsCars
        {
            get { return isShowOnlyViolationsCars; }
            set { SetProperty(ref isShowOnlyViolationsCars, value); }
        }

        private bool isExpanderWithCarDetailsVisible;
        public bool IsExpanderWithCarDetailsVisible
        {
            get { return isExpanderWithCarDetailsVisible; }
            set { SetProperty(ref isExpanderWithCarDetailsVisible, value); }
        }

        private bool isSettingsPopupVisible;
        public bool IsSettingsPopupVisible
        {
            get { return isSettingsPopupVisible; }
            set { SetProperty(ref isSettingsPopupVisible, value); }
        }

        private bool isViolationsWndVisible;
        public bool IsViolationsWndVisible
        {
            get { return isViolationsWndVisible; }
            set { SetProperty(ref isViolationsWndVisible, value); }
        }
        
        // Filter by vehicle type
        private Dictionary<string, bool> vehicleTypes;
        public Dictionary<string, bool> VehicleTypes
        {
            get { return vehicleTypes; }
            set { SetProperty(ref vehicleTypes, value); }
        }

        // Collection with filter
        public ObservableCollection<IntegratedVehicleInfo> VehicleFilteredList
        {
            get
            {
                IEnumerable<IntegratedVehicleInfo> dataList = new List<IntegratedVehicleInfo>();

                var trueVehiclesTypes = VehicleTypes?
                        .Where(types => types.Value)?.ToDictionary(obj => obj.Key, val => val.Value);

                if (string.IsNullOrWhiteSpace(SearchField))
                    dataList = ModuleData.SimpleDataForReport;
                else
                    dataList = ModuleData.SimpleDataForReport?.Where(x => x.StateNumber.Contains(SearchField));
                
                if (IsHideNullMileageCars == true)
                    dataList = dataList?.Where(x => x.PercentDifference != null)?.ToList();

                if (IsShowOnlyViolationsCars == true)
                    dataList = dataList?.Where(x => x.CountSpeedViolations > 0)?.ToList();
                
                if(trueVehiclesTypes != null)
                    dataList = dataList?.Where(x => trueVehiclesTypes.ContainsKey(x.Type.Trim()))?.ToList();

                if (dataList == null)
                    return new ObservableCollection<IntegratedVehicleInfo>();

                return new ObservableCollection<IntegratedVehicleInfo>(dataList);
            }
        }
        #endregion


        public OperationResultsViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            CanGoBack = true;
            CanGoNext = true;
            this.Title = "Результаты";

            OnCarChangedCmd = new DelegateCommand(OnCarChanged);
            LoadedCommand = new DelegateCommand(() => { UpdateFilterCars(); });
            OpenViolationsWndCommand = new DelegateCommand(OpenViolationsWnd);
            UpdateFilterCarsCommand = new DelegateCommand(UpdateFilterCars);
            UnCheckFilterTypeVehicleCommand = new DelegateCommand<string>(UnCheckFilterTypeVehicle);

            IsExpanderWithCarsVisible = true;

            
            GridConfiguration = new GridConfiguration();
        }


        #region Implementation Navigate
        public override void OnEnter()
        {
            base.OnEnter();
            if (ModuleData.SimpleDataForReport == null)
                ModuleData.SimpleDataForReport = new ObservableCollection<IntegratedVehicleInfo>(ModuleData.DetailsDataForReport);
            
            SelectedSpeedViolationInfoForChart = null;
            SelectedVehicleInfoInMilesChart = null;

            VehicleTypes = new Dictionary<string, bool>();
            foreach (var item in ModuleData.VehiclesByType)
            {
                VehicleTypes.Add(item.Key, true);
            }
            VehicleTypes = new Dictionary<string, bool>(VehicleTypes);
            RaisePropertyChanged(nameof(VehicleTypes));

        }

        public override void OnExit()
        {
            base.OnExit();
            //base.NormalizeWindow();
        }

        public override bool MoveBack()
        {
            return true;
        }
        
        public override async Task<bool> MoveNext()
        {
            return true;
        }
        #endregion


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

                IsViolationsWndVisible = false;

                await Task.Run(() => { 
                    PredictionChartContext = new PredictionChartViewModel(
                        PrepareData(sel.TripsSAP, sel.TripsWialon));
                });
            }
        }


        public DelegateCommand LoadedCommand { get; private set; }

        public DelegateCommand OpenViolationsWndCommand { get; private set; }
        private void OpenViolationsWnd()
        {
            this.IsViolationsWndVisible = !IsViolationsWndVisible;
        }

        public DelegateCommand UpdateFilterCarsCommand { get; private set; }
        private void UpdateFilterCars()
        {
            //if (VehicleTypes == null)
            //{
            //    VehicleTypes = new ConcurrentObservableDictionary<string, bool>();
            //    foreach (var item in ModuleData.VehiclesByType)
            //    {
            //        VehicleTypes.Add(item.Key, true);
            //    }
                
            //}
            RaisePropertyChanged(nameof(VehicleFilteredList));
        }

        public DelegateCommand<string> UnCheckFilterTypeVehicleCommand { get; private set; }
        protected virtual void UnCheckFilterTypeVehicle(string type)
        {
            if (VehicleTypes != null && VehicleTypes.ContainsKey(type))
                VehicleTypes[type] = !VehicleTypes[type];

            RaisePropertyChanged(nameof(VehicleTypes));
            RaisePropertyChanged(nameof(VehicleFilteredList));
        }
        #endregion


        #region Helpers
        private Dictionary<string, List<DateTimePoint>> PrepareData(IEnumerable<TripSAP> sap, IEnumerable<TripWialon> wialon)
        {
            if (sap == null || !sap.Any() ||
                wialon == null || !wialon.Any())
                return null;

            sap = sap.OrderBy(x => x.DepartureFromGarageDate.Date);
            wialon = wialon.OrderBy(x => x.Begin.Date);

            var datesDictSAP = sap.ToLookup(x => x.DepartureFromGarageDate.Date);
            var datesDictWln = wialon.ToLookup(x => x.Begin.Date);

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
                currDate <= endDate.Value.AddDays(1);)
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
