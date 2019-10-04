using DialogConstruction.Interfaces;
using Integration.ModuleGUI.Models;
using Integration.PartialViews.ViewModels;
using IntegrationSolution.Common.Enums;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Common.Models;
using IntegrationSolution.Common.ModulesExtension.Implementations;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using IntegrationSolution.Excel.Interfaces;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Unity;

namespace Integration.ModuleGUI.ViewModels
{
    public enum ChartDefinition
    {
        CarMileageStatisticsSAP,
        CarAverageMileageByTripStatisticsSAP
    }

    public enum DayOfWeek
    {
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 7,
    }

    public class OperationsViewModel : VMLocalBase
    {
        #region Variables
        private GridConfiguration gridConfiguration;
        public GridConfiguration GridConfiguration
        {
            get { return gridConfiguration; }
            set { SetProperty(ref gridConfiguration, value); }
        }

        private SeriesCollection carMileageStatisticsSAP;
        public SeriesCollection CarMileageStatisticsSAP
        {
            get { return carMileageStatisticsSAP; }
            set { SetProperty(ref carMileageStatisticsSAP, value); }
        }
        
        private SeriesCollection carAverageMileageByTripStatisticsSAP;
        public SeriesCollection CarAverageMileageByTripStatisticsSAP
        {
            get { return carAverageMileageByTripStatisticsSAP; }
            set { SetProperty(ref carAverageMileageByTripStatisticsSAP, value); }
        }

        private ObservableCollection<IVehicleSAP> _commonCars;
        public ObservableCollection<IVehicleSAP> CommonCars
        {
            get { return _commonCars; }
            set { SetProperty(ref _commonCars, value); }
        }

        private SeriesCollection driverWorkingDays;
        public SeriesCollection DriverWorkingDays
        {
            get { return driverWorkingDays; }
            set { SetProperty(ref driverWorkingDays, value); }
        }

        private bool isExpanderWithDriversVisible;
        public bool IsExpanderWithDriversVisible
        {
            get { return isExpanderWithDriversVisible; }
            set { SetProperty(ref isExpanderWithDriversVisible, value); }
        }

        private bool isSettingsPopupVisible;
        public bool IsSettingsPopupVisible
        {
            get { return isSettingsPopupVisible; }
            set { SetProperty(ref isSettingsPopupVisible, value); }
        }

        private bool isTopFilterPopupVisible;
        public bool IsTopFilterPopupVisible
        {
            get { return isTopFilterPopupVisible; }
            set { SetProperty(ref isTopFilterPopupVisible, value); }
        }

        private bool isDriversCarsPopupVisible;
        public bool IsDriversCarsPopupVisible
        {
            get { return isDriversCarsPopupVisible; }
            set { SetProperty(ref isDriversCarsPopupVisible, value); }
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
                RaisePropertyChanged(nameof(DriversFilteredList));
                IsAccessFilterFromListBox = IsAccessFilterFromListBox;
            }
        }

        private string searchChartField;
        public string SearchChartField
        {
            get { return searchChartField; }
            set
            {
                if (searchChartField == value)
                    return;

                SetProperty(ref searchChartField, value);
                SetDriversMainChartDataBySelectedTypeChart(SelectedIndexDriversMainCharts, value);
            }
        }

        public Dictionary<int, string> DriversMainCharts { get; set; } = new Dictionary<int, string>()
            {
                { 0, "Показатели по километражу" },
                { 1, "Показатели по продуктивности" },
                { 2, "Показатели по среднему километражу за поездку" },
                { 3, "Показатели по количеству поездок" },
                { 4, "Показатели по самым длинным поездкам" },
                { 5, "Показатели по самым коротким поездкам" }
            };
        private int selectedIndexDriversMainCharts;
        public int SelectedIndexDriversMainCharts
        {
            get { return selectedIndexDriversMainCharts; }
            set
            {
                SetProperty(ref selectedIndexDriversMainCharts, value);
                SetDriversMainChartDataBySelectedTypeChart(value, SearchChartField);
            }
        }

        // MAIN CHART
        private ChartValues<Driver> driversStatisticsSAP;
        public ChartValues<Driver> DriversStatisticsSAP
        {
            get { return driversStatisticsSAP; }
            set { SetProperty(ref driversStatisticsSAP, value); }
        }

        private ChartsVMBase predictionDriversChartContext;
        public ChartsVMBase PredictionDriversChartContext
        {
            get { return predictionDriversChartContext; }
            set { SetProperty(ref predictionDriversChartContext, value); }
        }

        private Driver selectedDriverChart;
        public Driver SelectedDriverChart
        {
            get { return selectedDriverChart; }
            set { SetProperty(ref selectedDriverChart, value); }
        }

        #region Total Drivers Statistics
        public int? TotalTripsAtAll
        {
            get => ModuleData?.DriverCollection?.Sum(x => x.CountTrips);
        }

        public double? TotalMileageAtAll
        {
            get => Math.Round(ModuleData?.DriverCollection?.Sum(x => x.TotalMileage) ?? 0, 2);
        }

        public double? TotalAvgMileagePerTripAtAll
        {
            get => Math.Round(ModuleData?.DriverCollection?.Average(x => x.AvarageMileagePerTrip) ?? 0, 2);
        }

        public double? TotalMinTripAtAll
        {
            get => Math.Round(ModuleData?.DriverCollection?.Min(x => x.MinTripMileage.Key) ?? 0, 2);
        }

        public double? TotalMaxTripAtAll
        {
            get => Math.Round(ModuleData?.DriverCollection?.Max(x => x.MaxTripMileage.Key) ?? 0, 2);
        }
        #endregion

        public ObservableCollection<string> Labels { get; set; }

        private Func<double, string> formatter;
        public Func<double, string> Formatter
        {
            get { return formatter; }
            set { SetProperty(ref formatter, value); }
        }

        private object mapper;
        public object Mapper
        {
            get { return mapper; }
            set { SetProperty(ref mapper, value); }
        }


        #region Filters for top lists
        private bool isShowCommonTop10;
        public bool IsShowCommonTop10
        {
            get { return isShowCommonTop10; }
            set { SetProperty(ref isShowCommonTop10, value); InitializeTopCharts(); }
        }

        private bool isShowLast10;
        public bool IsShowLast10
        {
            get { return isShowLast10; }
            set { SetProperty(ref isShowLast10, value); InitializeTopCharts(); }
        }
        #endregion


        #region Filters for driver list
        private bool isShowOnlyDrivers;
        public bool IsShowOnlyDrivers
        {
            get { return isShowOnlyDrivers; }
            set { SetProperty(ref isShowOnlyDrivers, value); }
        }
        #endregion


        private bool isAccessFilterFromListBox;
        public bool IsAccessFilterFromListBox
        {
            get { return isAccessFilterFromListBox; }
            set
            {
                SetProperty(ref isAccessFilterFromListBox, value);
                if (value)
                {
                    SetDriversMainChartDataBySelectedTypeChart(SelectedIndexDriversMainCharts, SearchField);
                }
                else
                {
                    SetDriversMainChartDataBySelectedTypeChart(SelectedIndexDriversMainCharts, SearchChartField);
                }
            }
        }

        // Filter by vehicle type
        //private Dictionary<string, bool> vehicleTypes;
        //public Dictionary<string, bool> VehicleTypes
        //{
        //    get { return vehicleTypes; }
        //    set { SetProperty(ref vehicleTypes, value); }
        //}

        // Collection with filter (FOR LISTBOX)
        public ObservableCollection<Driver> DriversFilteredList
        {
            get
            {
                IEnumerable<Driver> dataList = new List<Driver>();
                if (string.IsNullOrWhiteSpace(SearchField))
                    dataList = ModuleData.DriverCollection?.ToList();
                else
                {
                    var search = SearchField.ToLower();

                    dataList = ModuleData.DriverCollection?
                        .Where(x => x.LastName.ToLower().Contains(search) || x.FirstName.ToLower().Contains(search)
                        || x.Patronymic.ToLower().Contains(search) || x.UnitNumber.ToLower().Contains(search));
                }
                if (IsShowOnlyDrivers == true)
                {
                    var avgTripsAtAll = (int)ModuleData.DriverCollection.Select(x => x.CountTrips).Average();
                    var avgDriversTrips = (int)(ModuleData.DriverCollection.Where(x => x.CountTrips > avgTripsAtAll).Select(x => x.CountTrips).Average() * 0.65);
                    dataList = dataList?.Where(x => x.CountTrips >= avgDriversTrips)?.ToList();
                }

                //if (IsShowOnlyViolationsCars == true)
                //    dataList = dataList?.Where(x => x.CountSpeedViolations > 0)?.ToList();

                if (dataList == null)
                    return new ObservableCollection<Driver>();

                return new ObservableCollection<Driver>(dataList.OrderByDescending(x => x.CountTrips));
            }
        }
        #endregion


        #region Ctor
        private readonly IDialogManager _dialogManager;

        public OperationsViewModel(IDialogManager dialogManager, IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            this.Title = "Операции";
            this.CanGoBack = true;
            this.CanGoNext = false;

            IsExpanderWithDriversVisible = true;
            IsSettingsPopupVisible = false;

            WriteTotalStatisticsInFileCommand = new DelegateCommand(WriteTotalStatisticsJob);
            CheckDifferenceOfTotalSpeedCommand = new DelegateCommand(CheckDifference);
            ShowDetailsOnSAPCommand = new DelegateCommand(ShowSAPDetailsWndCmd);
            ClickDataCommand = new DelegateCommand<ChartPoint>(ClickDataCmd);
            OnDriverChangedCmd = new DelegateCommand(OnDriverChanged);
            OpenDriversCarsPopupCommand = new DelegateCommand(OpenDriversCarsPopup);
            ShowDriversMainChartCommand = new DelegateCommand(ShowDriversMainChart);
            ShowTotalCommand = new DelegateCommand<string>(ShowTotal);
            UpdateFilterDriversCommand = new DelegateCommand(UpdateFilterDrivers);
            ShowLast10Command = new DelegateCommand(ShowLast10);

            GridConfiguration = new GridConfiguration();

            _dialogManager = dialogManager;
        }
        #endregion


        #region Moves next/back
        public override bool MoveBack() => this.CanGoBack;

        public override async Task<bool> MoveNext()
        {
            if (ModuleData.Vehicles == null)
            {
                var progress = await InitializeCars();
                await progress.CloseAsync();
            }

            if (ModuleData.DetailsDataForReport == null && ModuleData.SimpleDataForReport == null)
                return false;

            this.IsFinished = true;
            return CanGoNext;
        }
        #endregion


        #region Commands
        public DelegateCommand WriteTotalStatisticsInFileCommand { get; private set; }
        protected async void WriteTotalStatisticsJob()
        {
            var wnd = (MetroWindow)Application.Current.MainWindow;

            var fuelPrices = await _dialogManager.ShowDialogAsync<FuelPrice>(DialogNamesEnum.FuelPriceDialog);
            if (fuelPrices == null)
                return;

            var progress = await InitializeCars();

            await Task.Run(() =>
            {
                try
                {
                    var percentage = 75;

                    progress.SetTitle($"Запись в файл");
                    if (percentage < 90)
                        percentage += 9;
                    progress.SetProgress(percentage / 100);
                    (this.ModuleData.ExcelMainFile as ICarOperations).WriteInHeadersAndDataForTotalResult(ModuleData.Vehicles);

                    if (percentage < 90)
                        percentage += 9;
                    progress.SetProgress(percentage / 100);

                    (this.ModuleData.ExcelMainFile as ICarOperations).WriteInTotalResultOfEachStructure(ModuleData.Vehicles);

                    progress.SetTitle($"Сохранение");
                    progress.SetProgress(0.99);
                    ModuleData.ExcelMainFile.Save();
                    this.CanGoNext = true;
                    progress.SetProgress(1);
                }
                catch (Exception ex)
                {
                    this.Error = new IntegrationSolution.Common.Entities.Error()
                    {
                        IsError = true,
                        ErrorDescription = ex.Message
                    };
                }
            });

            if (progress.IsOpen)
                await progress.CloseAsync();

            base.NotifyOnUpdateEvents();

            if (this.Error == null || !this.Error.IsError)
            {
                NotifySuccessAndOpenFile(ModuleData.PathToMainFile);
            }
        }


        public DelegateCommand CheckDifferenceOfTotalSpeedCommand { get; private set; }
        [STAThread]
        protected async void CheckDifference()
        {
            var wnd = (MetroWindow)Application.Current.MainWindow;
            string nameReport = "";

            var desicion = await wnd.ShowMessageAsync("Вы хотите продолжить?", "Данная процедура может занять некоторое время.", MessageDialogStyle.AffirmativeAndNegative);
            if (desicion != MessageDialogResult.Affirmative)
                return;

            var tmp_progress = await wnd.ShowProgressAsync("Подождите пожалуйста", "Подготовка данных...");

            // Get cars from Wialon
            ICollection<CarWialon> wialonCars = null;

            wialonCars = await Task.Run(() => { return _wialonContext.GetCarsEnumarable(); });

            if (tmp_progress.IsOpen)
                await tmp_progress.CloseAsync();

            if (wialonCars == null)
            {
                await wnd.ShowMessageAsync("Ошибка!", "Проверьте подключение к навигационной системе Wialon.");
                return;
            }
            else
                ModuleData.VehiclesNavigate = new ObservableCollection<CarWialon>(wialonCars);


            var datesFromToContext = await _dialogManager.ShowDialogAsync<DatesFromToContext>(DialogNamesEnum.DatesFromTo);
            if (datesFromToContext == null)
                return;

            #region Get BadPercent
            double avaliablePercent = 0;
            do
            {
                try
                {
                    var percInput = await wnd.ShowInputAsync("Допустимый процент расхождения", "Например: 3.5");
                    if (percInput == null)
                        return;
                    avaliablePercent = Double.Parse(percInput.Replace('.', ',').Trim());
                    if (avaliablePercent <= 0)
                        throw new Exception("Внимательней, число должно быть больше от 0.");
                }
                catch (Exception ex)
                {
                    wnd.ShowModalMessageExternal("Ошибка. Повторите ввод.", ex.Message);
                }
            } while (avaliablePercent <= 0);
            #endregion

            var progress = await InitializeCars();

            #region Filling Vehicles Collections
            await Task.Run(() =>
             {
                 ModuleData.VehiclesExcelDistinctWialon = new ObservableCollection<IVehicleSAP>(
                     ModuleData.Vehicles.Where(
                         x => ModuleData.VehiclesNavigate
                         .FirstOrDefault(
                             wialon => wialon.StateNumber.ToLower().Contains(x.StateNumber.ToLower())) == null));
                 ModuleData.VehiclesWialonDistinctExcel = new ObservableCollection<CarWialon>(
                     ModuleData.VehiclesNavigate.Where(
                         wialon => ModuleData.Vehicles.FirstOrDefault(
                             veh => wialon.StateNumber.ToLower().Contains(veh.StateNumber.ToLower())) == null));
             });
            #endregion

            await Task.Run(async () =>
            {
                try
                {
                    ModuleData.SimpleDataForReport = null;
                    ModuleData.DetailsDataForReport = null;

                    if (!datesFromToContext.IsWithDetails)
                    {
                        ModuleData.SimpleDataForReport = new ObservableCollection<IntegratedVehicleInfo>(await this.GetVehicleInfos<IntegratedVehicleInfo>(progress, datesFromToContext));
                        if (ModuleData.SimpleDataForReport == null || !ModuleData.SimpleDataForReport.Any())
                            throw new Exception("Данные отсутствуют.\nПопробуйте выбрать другой период или повторите попытку позже.");
                    }
                    else
                    {
                        ModuleData.DetailsDataForReport = new ObservableCollection<IntegratedVehicleInfoDetails>(await this.GetVehicleInfos<IntegratedVehicleInfoDetails>(progress, datesFromToContext));
                        if (ModuleData.DetailsDataForReport == null || !ModuleData.DetailsDataForReport.Any())
                            throw new Exception("Данные отсутствуют.\nПопробуйте выбрать другой период или повторите попытку позже.");
                    }

                    var percentage = 95;
                    progress.SetTitle("Сохранение");
                    progress.SetProgress(percentage / 100);

                    this.CanGoNext = true;

                    await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        Microsoft.Win32.SaveFileDialog fileDialog = new Microsoft.Win32.SaveFileDialog
                        {
                            Title = "Создание отчёта...",
                            ValidateNames = true,
                            CheckPathExists = true,
                            DefaultExt = ".xlsx | .xls",
                            Filter = "Excel document (.xlsx)|*.xlsx|Excel document (.xls)|*.xls|All files (*.*)|*.*"
                        };
                        
                        if (fileDialog.ShowDialog() != true)
                            return;
                        else
                            nameReport = fileDialog.FileName;

                        if (!datesFromToContext.IsWithDetails)
                            _container.Resolve<IExcelWriter>().CreateReportDiffMileage(fileDialog.FileName,
                                    ModuleData.SimpleDataForReport.OrderByDescending(x => x.PercentDifference).ToList(), avaliablePercent,
                                    ModuleData.VehiclesExcelDistinctWialon.ToList(),
                                    ModuleData.VehiclesWialonDistinctExcel.ToList());
                        else
                            _container.Resolve<IExcelWriter>().CreateReportDiffMileageWithDetails(fileDialog.FileName,
                                    ModuleData.DetailsDataForReport.OrderByDescending(x => x.PercentDifference).ToList(), avaliablePercent,
                                    ModuleData.VehiclesExcelDistinctWialon.ToList(),
                                    ModuleData.VehiclesWialonDistinctExcel.ToList());
                    }));
                    progress.SetProgress(1);
                }
                catch (Exception ex)
                {
                    this.Error = new IntegrationSolution.Common.Entities.Error()
                    {
                        IsError = true,
                        ErrorDescription = ex.Message
                    };
                }

            });

            if (progress.IsOpen)
                await progress.CloseAsync();

            base.NotifyOnUpdateEvents();

            if (this.Error == null || !this.Error.IsError)
            {
                NotifySuccessAndOpenFile(nameReport);
            }
        }


        public DelegateCommand ShowDetailsOnSAPCommand { get; private set; }
        protected async void ShowSAPDetailsWndCmd()
        {
            try
            {
                var progress = await InitializeCars();
                await progress.CloseAsync();

                ModuleData.Vehicles = new ObservableCollection<IVehicleSAP>(ModuleData.Vehicles);
                Error = new IntegrationSolution.Common.Entities.Error()
                {
                    IsError = false,
                    ErrorDescription = "Обновлено"
                };
            }
            catch (Exception ex)
            {
                Error = new IntegrationSolution.Common.Entities.Error()
                {
                    IsError = true,
                    ErrorDescription = ex.Message
                };
            }
        }


        public DelegateCommand<ChartPoint> ClickDataCommand { get; private set; }
        protected async void ClickDataCmd(ChartPoint chartPoint)
        {
            StringBuilder msg = new StringBuilder();

            var car = ModuleData.Vehicles.Where(x => chartPoint?.SeriesView.Title.Contains(x.StateNumber) ?? false).FirstOrDefault();
            if (car != null)
            {
                if (!string.IsNullOrWhiteSpace(car.Department))
                    msg.AppendLine($"Служба/отдел:\t{car.Department}");

                if (!string.IsNullOrWhiteSpace(car.StructureName))
                    msg.AppendLine($"Структурное подразделение:\t{car.StructureName}");

                if (car.TripResulted != null)
                {
                    msg.AppendLine($"Количество выездов:\t{car.CountTrips}");
                    msg.AppendLine($"Пробег за период:\t{car.TripResulted.TotalMileage} км");
                    msg.AppendLine($"Средний пробег за поездку:\t{Math.Round((car.TripResulted.TotalMileage / car.CountTrips.Value), 2)} км/поездка");
                }

                await _dialogManager.ShowMessageBox($"{car.UnitModel}\t{car.StateNumber}", msg.ToString());
            }
            else
            {
                var driver = chartPoint.Instance as Driver;
                if (driver == null)
                    return;

                var avgTripsAtAll = (int)ModuleData.DriverCollection.Select(x => x.CountTrips).Average();
                var avgDriversTrips = (int)(ModuleData.DriverCollection.Where(x => x.CountTrips > avgTripsAtAll).Select(x => x.CountTrips).Average() * 0.65);
                var avgMileage = ModuleData.DriverCollection.Where(x => x.CountTrips >= avgDriversTrips).Select(x => x.AvarageMileagePerTrip).Average();

                msg.AppendLine($"Всего поездок за период:\t{driver.CountTrips}");
                msg.AppendLine($"Использовано транспортных средств:\t{driver.CountCars}");
                msg.AppendLine();
                msg.AppendLine($"Всего километраж за период:\t{Math.Round((driver.TotalMileage), 2)} км");
                msg.AppendLine($"Средний километраж за поездку:\t{Math.Round((driver.AvarageMileagePerTrip), 2)} км/поездка");
                msg.AppendLine($"Самая длинная поездка:\t{driver.MaxTripMileage.Key} км\t({driver.MaxTripMileage.Value.ToShortDateString()})");
                msg.AppendLine($"Самая коротка поездка:\t{driver.MinTripMileage.Key} км\t({driver.MinTripMileage.Value.ToShortDateString()})");
                msg.AppendLine($"Показатель продуктивности:\t{driver.GetEffectivityPercent(avgMileage)}%");
                msg.AppendLine();
                msg.AppendLine();
                msg.AppendLine("* Самых длинных и коротких поездок с одинаковым километражем может быть несколько. Отображается первая найденная.");

                await _dialogManager.ShowMessageBox($"{driver.ToString()}\t({driver.UnitNumber})", msg.ToString());
            }
        }


        public DelegateCommand OpenDriversCarsPopupCommand { get; private set; }
        private void OpenDriversCarsPopup()
        {
            this.IsDriversCarsPopupVisible = !IsDriversCarsPopupVisible;
        }


        public DelegateCommand ShowDriversMainChartCommand { get; private set; }
        private void ShowDriversMainChart()
        {
            SelectedDriverChart = null; //ModuleData.DriverCollection.First().
        }


        public DelegateCommand<string> ShowTotalCommand { get; private set; }
        protected async void ShowTotal(string TypeOfTotal)
        {
            if (string.IsNullOrWhiteSpace(TypeOfTotal))
                return;
            StringBuilder msg = new StringBuilder();
            String Title = string.Empty;
            List<Driver> drivers = null;

            switch (TypeOfTotal)
            {
                case nameof(TotalMinTripAtAll):
                    #region
                    drivers = ModuleData.DriverCollection?.Where(x => Math.Abs(x.MinTripMileage.Key - TotalMinTripAtAll.Value) < 0.1)?.ToList();

                    if (drivers == null || !drivers.Any())
                        return;

                    Title = $"Самая короткая поездка:\t{TotalMinTripAtAll} км";

                    for (int i = 0; i < drivers.Count; i++)
                    {
                        msg.AppendLine($"{drivers[i]}\t\t{drivers[i].MinTripMileage.Value.ToShortDateString()}");
                    }
                    break;
                #endregion

                case nameof(TotalMaxTripAtAll):
                    #region
                    drivers = ModuleData.DriverCollection?.Where(x => Math.Abs(x.MaxTripMileage.Key - TotalMaxTripAtAll.Value) < 0.1)?.ToList();

                    if (drivers == null || !drivers.Any())
                        return;

                    Title = $"Самая длинная поездка:\t{TotalMaxTripAtAll} км";

                    for (int i = 0; i < drivers.Count; i++)
                    {
                        msg.AppendLine($"{drivers[i]}\t\t{drivers[i].MaxTripMileage.Value.ToShortDateString()}");
                    }
                    break;
                #endregion

                case nameof(TotalMileageAtAll):
                    #region
                    var max = ModuleData.DriverCollection?.Max(x => x.TotalMileage);
                    drivers = ModuleData.DriverCollection?.Where(x => Math.Abs(x.TotalMileage - max.Value) < 0.1)?.ToList();

                    if (drivers == null || !drivers.Any())
                        return;

                    Title = $"Самый большой километраж за период:\t{max} км";

                    for (int i = 0; i < drivers.Count; i++)
                    {
                        msg.AppendLine($"{drivers[i]}\t\t Всего поездок: {drivers[i].CountTrips}");
                    }
                    break;
                #endregion

                case nameof(TotalAvgMileagePerTripAtAll):
                    #region
                    var avgMax = ModuleData.DriverCollection?.Max(x => x.AvarageMileagePerTrip);
                    var avgMin = ModuleData.DriverCollection?.Min(x => x.AvarageMileagePerTrip);
                    var driversMax = ModuleData.DriverCollection?.Where(x => Math.Abs(x.AvarageMileagePerTrip - avgMax.Value) < 0.1)?.ToList();
                    var driversMin = ModuleData.DriverCollection?.Where(x => Math.Abs(x.AvarageMileagePerTrip - avgMin.Value) < 0.1)?.ToList();
                    Title = "";
                    if (driversMax != null && driversMax.Any())
                    {
                        Title = $"Самый большой средний километраж за поездку:\t{avgMax} км/поездка {Environment.NewLine}";

                        for (int i = 0; i < driversMax.Count; i++)
                        {
                            msg.AppendLine($"Лидеры по среднему пробегу:");
                            msg.AppendLine($"{driversMax[i]}\t\t Всего поездок: {driversMax[i].CountTrips} ({driversMax[i].TotalMileage} км)");
                        }
                    }

                    if (driversMin != null && driversMin.Any())
                    {
                        Title += $"Самый малый средний километраж за поездку:\t{avgMin} км/поездка";

                        msg.AppendLine();
                        msg.AppendLine();
                        msg.AppendLine($"Аутсайдеры по среднему пробегу:");

                        for (int i = 0; i < driversMin.Count; i++)
                        {
                            msg.AppendLine($"{driversMin[i]}\t\t Всего поездок: {driversMin[i].CountTrips} ({driversMin[i].TotalMileage} км)");
                        }
                    }

                    if (string.IsNullOrWhiteSpace(Title))
                        return;
                    break;
                #endregion

                default:
                    return;
            }
            await _dialogManager.ShowMessageBox(Title, msg.ToString());
        }


        public DelegateCommand OnDriverChangedCmd { get; private set; }
        [STAThreadAttribute]
        protected async void OnDriverChanged()
        {
            if (SelectedDriverChart == null)
                return;

            if (SelectedDriverChart.HistoryDrive != null)
            {
                await Task.Run(() =>
                {
                    PredictionDriversChartContext = new PredictionChartViewModel(
                        PrepareData(SelectedDriverChart.HistoryDrive.SelectMany(x => x.Value)))
                    { Title = "График водителя" };
                });

                DriverWorkingDays = InitializeChartsData(SelectedDriverChart.HistoryDrive.SelectMany(x => x.Value)
                    , chartPoint => string.Format("{0} км ({1:P})", chartPoint.Y, chartPoint.Participation));
            }
        }


        public DelegateCommand UpdateFilterDriversCommand { get; private set; }
        private void UpdateFilterDrivers()
        {
            RaisePropertyChanged(nameof(DriversFilteredList));

            if (IsAccessFilterFromListBox)
                IsAccessFilterFromListBox = IsAccessFilterFromListBox; // need to call property

        }
        

        public DelegateCommand ShowLast10Command { get; private set; }
        private void ShowLast10()
        {
            var wnd = (MetroWindow)Application.Current.MainWindow;

            var topTotal = new List<IVehicleSAP>();
            var topAvg = new List<IVehicleSAP>();
            
            foreach (var item in ModuleData.VehicleTypes?.Where(x => x.Value == true).Select(x => x.Key))
            {
                topTotal.AddRange(ModuleData.VehiclesByType[item]);
                topAvg.AddRange(ModuleData.VehiclesByType[item]);
            }

            topTotal = topTotal.Where(x => x.TripResulted != null)?.OrderBy(x => x.TripResulted.TotalMileage).Take(10).ToList();
            topAvg = topAvg.Where(x => x.TripResulted != null)?.OrderBy(x => (x?.TripResulted?.TotalMileage / x.CountTrips ?? -1)).Take(10).ToList();

            StringBuilder msg = new StringBuilder();

            msg.AppendLine("Топ 10 аутсайдеров по пробегу:" + Environment.NewLine);
            topTotal.ForEach(x => msg.AppendLine($"{(topTotal.IndexOf(x) + 1).ToString()}. {x.StateNumber}\t({x.UnitModel} / {x.Type})\t\t{x.TripResulted?.TotalMileage}км"));


            msg.AppendLine(Environment.NewLine + Environment.NewLine + "Топ 10 аутсайдеров по среднему пробегу за поездку:" + Environment.NewLine);
            topAvg.ForEach(x => msg.AppendLine($"{(topAvg.IndexOf(x) + 1).ToString()}. {x.StateNumber}\t({x.UnitModel} / {x.Type})\t\t{(Math.Round((x.TripResulted?.TotalMileage / x.CountTrips ?? -1), 2)).ToString()}км"));

            wnd.ShowModalMessageExternal("Список аутсайдеров", msg.ToString());
        }
        
        protected override void UnCheckFilterTypeVehicle(string type)
        {
            ModuleData.ChangeStateType(type);
            InitializeTopCharts();
        }
        #endregion

        #region Helpers
        private async Task<ProgressDialogController> InitializeCars()
        {
            Error = null;
            var wnd = (MetroWindow)Application.Current.MainWindow;
            var progress = await wnd.ShowProgressAsync("Подождите...", "Инициализация файлов");

            await Task.Run(() =>
            {
                try
                {
                    double percentage = 0;
                    progress.SetTitle("Инициализация транспортных средств");
                    percentage += 5;
                    progress.SetProgress(percentage / 100);

                    var cars = (this.ModuleData.ExcelMainFile as ICarOperations)?.GetVehicles()?.ToList();
                    var storageData = (this.ModuleData.ExcelPathListFile as ICarOperations);

                    progress.SetTitle($"Инициализация транспортных средств. На очереди {cars.Count} обьектов.");

                    if (cars == null || storageData == null || !cars.Any())
                        throw new Exception("Ошибка. Попробуйте вернуться и загрузить файлы по новой.");

                    int countCarsPerOnePercent = (cars.Count / 70) + 1;
                    for (int i = 0; i < cars.Count; i++)
                    {
                        var car = cars.ElementAtOrDefault(i);
                        storageData.SetFieldsOfVehicleByAvaliableData(ref car);

                        if (i % countCarsPerOnePercent == 0)
                        {
                            progress.SetTitle($"Инициализация транспортных средств: Инициализация {i} обьекта из {cars.Count}.");
                            progress.SetProgress(++percentage / 100);
                        }
                    }

                    ModuleData.Vehicles = new ObservableCollection<IVehicleSAP>(cars);
                }
                catch (Exception ex)
                {
                    this.Error = new IntegrationSolution.Common.Entities.Error()
                    {
                        IsError = true,
                        ErrorDescription = ex.Message
                    };
                }
            });

            if (ModuleData.Vehicles.Any())
            {
                // Initialize Drivers
                InitializeDrivers(ModuleData.Vehicles.AsEnumerable());

                ModuleData.VehicleTypes = new Dictionary<string, bool>();
                foreach (var item in ModuleData.VehiclesByType)
                {
                    ModuleData.VehicleTypes.Add(item.Key, true);
                }

                InitializeTopCharts();
            }

            return progress;
        }

        private void InitializeTopCharts()
        {
            var topTotal = new List<IVehicleSAP>();
            var topAvg = new List<IVehicleSAP>();

            foreach (var item in ModuleData.VehicleTypes?.Where(x => x.Value == true).Select(x => x.Key))
            {
                topTotal.AddRange(ModuleData.VehiclesByType[item]);
                topAvg.AddRange(ModuleData.VehiclesByType[item]);
            }

            if (IsShowLast10)
            {
                topTotal = topTotal.Where(x => x.TripResulted != null)?.OrderBy(x => x.TripResulted.TotalMileage).Take(10).ToList();
                topAvg = topAvg.Where(x => x.TripResulted != null)?.OrderBy(x => (x?.TripResulted?.TotalMileage / x.CountTrips ?? -1)).Take(10).ToList();
            }
            else
            {
                topTotal = topTotal.Where(x => x.TripResulted != null)?.OrderByDescending(x => x?.TripResulted?.TotalMileage).Take(10).ToList();
                topAvg = topAvg.Where(x => x.TripResulted != null)?.OrderByDescending(x => (x?.TripResulted?.TotalMileage / x.CountTrips ?? -1)).Take(10).ToList();
            }

            var resTotal = topTotal.OrderByDescending(x => x?.TripResulted?.TotalMileage).Take(10);
            var resAvg = topAvg.OrderByDescending(x => (x?.TripResulted?.TotalMileage / x.CountTrips ?? -1)).Take(10);
            
            CommonCars = new ObservableCollection<IVehicleSAP>(resTotal.Intersect(resAvg));

            if (IsShowCommonTop10)
            {
                resTotal = resTotal.Where(x => CommonCars.Contains(x));
                resAvg = resAvg.Where(x => CommonCars.Contains(x));
            }
            
            CarMileageStatisticsSAP = InitializeChartsData(
                resTotal
                    , ChartDefinition.CarMileageStatisticsSAP
                    , chartPoint => string.Format("{0} км", chartPoint.Y));

            CarAverageMileageByTripStatisticsSAP = InitializeChartsData(
                resAvg
                , ChartDefinition.CarAverageMileageByTripStatisticsSAP
                , chartPoint => string.Format("{0} км", chartPoint.Y));

        }

        // Initialize collection for charts and returns by 
        private SeriesCollection InitializeChartsData(IEnumerable<IVehicleSAP> vehicles, ChartDefinition chart, Func<ChartPoint, string> Label)
        {
            SeriesCollection data = new SeriesCollection();

            foreach (var item in vehicles)
            {
                ChartValues<double> vals = null;

                switch (chart)
                {
                    case ChartDefinition.CarMileageStatisticsSAP:
                        vals = new ChartValues<double>(new[] { item.TripResulted?.TotalMileage ?? 0 });
                        break;
                    case ChartDefinition.CarAverageMileageByTripStatisticsSAP:
                        vals = new ChartValues<double>(new[] { Math.Round((item.TripResulted?.TotalMileage / item.CountTrips ?? -1), 2) });
                        break;
                    default:
                        return null;
                }

                data.Add(new PieSeries()
                {
                    Title = $"{item.StateNumber} ({item.UnitModel})",
                    Values = vals,
                    DataLabels = true,
                    LabelPoint = Label
                });
            }

            return data;

        }

        // Initialize collection for working days of driver
        private SeriesCollection InitializeChartsData(IEnumerable<TripSAP> trips, Func<ChartPoint, string> Label)
        {
            SeriesCollection data = new SeriesCollection();

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek))
                              .OfType<DayOfWeek>()
                              .ToList())
            {

                var common = trips.Where(x => x.DepartureFromGarageDate.DayOfWeek.ToString().Equals(day.ToString()));

                if (!common.Any())
                    continue;

                ChartValues<double> vals = new ChartValues<double>(new[]
                {
                    Math.Round(common?.Sum(x => x.TotalMileage) ?? 0, 2)
                });

                data.Add(new PieSeries()
                {
                    Title = $"{day.ToString()}",
                    Values = vals,
                    DataLabels = true,
                    LabelPoint = Label
                });
            }

            return data;

        }


        private async void InitializeDrivers(IEnumerable<IVehicleSAP> vehicles)
        {
            if (vehicles == null || !vehicles.Any())
                return;

            await Task.Run(() =>
            {
                var drivers_trips = ModuleData.Vehicles?.Where(x => x.Trips != null)?.SelectMany(x => x.Trips)
                    .ToDictionary(x => x.Driver);

                base.ModuleData.DriverCollection = new ConcurrentObservableCollection<Driver>();
                var compareTrips = new CompareTripSAP();

                foreach (var item in drivers_trips)
                {
                    var driver = ModuleData.DriverCollection.FirstOrDefault(x => x.LastName == item.Key.LastName && x.UnitNumber == item.Key.UnitNumber);

                    var car = ModuleData.Vehicles.Where(x => x.Trips != null).FirstOrDefault(x => x.Trips.Contains(item.Value, compareTrips));

                    if (driver != null)
                    {
                        if (driver.HistoryDrive == null)
                            driver.HistoryDrive = new ConcurrentObservableDictionary<IVehicle, List<TripSAP>>(new CompareVehicles());
                        if (car != null)
                        {
                            var car_atCollection = driver.HistoryDrive.FirstOrDefault(x => x.Key.StateNumber == car.StateNumber);
                            if (car_atCollection.Key != null)
                            {
                                if (car_atCollection.Value == null)
                                    car_atCollection = new KeyValuePair<IVehicle, List<TripSAP>>(car_atCollection.Key, new List<TripSAP>());
                            }
                            else
                                car_atCollection = new KeyValuePair<IVehicle, List<TripSAP>>(car, new List<TripSAP>());

                            car_atCollection.Value.Add(item.Value);
                            driver.HistoryDrive.Add(car, car_atCollection.Value);
                        }
                    }
                    else
                    {
                        var new_driver = item.Key;
                        new_driver.HistoryDrive = new ConcurrentObservableDictionary<IVehicle, List<TripSAP>>(new CompareVehicles());
                        if (car != null)
                        {
                            new_driver.HistoryDrive.Add(car, new List<TripSAP>() { item.Value });
                            ModuleData.DriverCollection.Add(new_driver);
                        }
                    }
                }

                SelectedIndexDriversMainCharts = 0;
                RaisePropertyChanged(nameof(DriversFilteredList));
                UpdateTotalStatistics();
            });
        }


        private void NotifySuccessAndOpenFile(string path)
        {
            var wnd = (MetroWindow)Application.Current.MainWindow;
            var userDesicion = wnd.ShowModalMessageExternal("Успех!",
                    "Результаты успешно сохранены.\nЖелаете ли просмотреть результаты?",
                    MessageDialogStyle.AffirmativeAndNegative);
            try
            {
                if (userDesicion == MessageDialogResult.Affirmative)
                    System.Diagnostics.Process.Start(path);
            }
            catch (Exception ex)
            {
                this.Error = new IntegrationSolution.Common.Entities.Error()
                {
                    IsError = true,
                    ErrorDescription = ex.Message
                };
            }
        }


        private void SetDriversMainChartDataBySelectedTypeChart(int index, string search_Field)
        {
            try
            {
                int CountTake = 20;

                List<Driver> baseCollection = null;
                if (IsAccessFilterFromListBox)
                    baseCollection = DriversFilteredList.ToList();
                else
                    baseCollection = ModuleData.DriverCollection.ToList();

                if (DriversStatisticsSAP == null)
                {
                    Mapper = Mappers.Xy<Driver>()
                    .X((driver, ind) => ind)
                    .Y(driver => driver.HistoryDrive.Select(z => z.Value.Sum(k => k.TotalMileage)).FirstOrDefault());

                    //var record = ModuleData.DriverCollection.OrderByDescending(x => x.HistoryDrive.Select(tr => tr.Value.Sum(ml => ml.TotalMileage)).FirstOrDefault())
                    //        .Take(CountTake).ToArray();
                    var record = baseCollection.OrderByDescending(x => x.HistoryDrive.Select(tr => tr.Value.Sum(ml => ml.TotalMileage)).FirstOrDefault())
                            .Take(CountTake).ToArray();
                    DriversStatisticsSAP = record.AsChartValues();
                    Labels = new ObservableCollection<string>(record.Select(x => $"{x.LastName} {x.FirstName}."));
                    Formatter = val => (val).ToString() + " км";
                    return;
                }

                string search = (search_Field ?? string.Empty).ToLower();
                Driver[] records = new Driver[0];

                // Indexes from DriversMainCharts field
                switch (index)
                {
                    case 1:
                        // getting the average scale of CountTrips between all drivers (clip 50% of trips) - Left border of min trips
                        //var avgTripsAtAll = (int)ModuleData.DriverCollection.Select(x => x.CountTrips).Average();
                        var avgTripsAtAll = (int)baseCollection.Select(x => x.CountTrips).Average();

                        // getting the average scale of CountTrips between second part of drivers (clip 75% of clips)
                        // cut off not drivers - change left border of min trips
                        //var avgDriversTrips = (int)(ModuleData.DriverCollection.Where(x => x.CountTrips > avgTripsAtAll).Select(x => x.CountTrips).Average()*0.65);
                        var avgDriversTrips = (int)(baseCollection.Where(x => x.CountTrips > avgTripsAtAll).Select(x => x.CountTrips).Average() * 0.65);

                        //var avgMileage = ModuleData.DriverCollection.Where(x => x.CountTrips >= avgDriversTrips).Select(x => x.AvarageMileagePerTrip).Average();
                        var avgMileage = baseCollection.Where(x => x.CountTrips >= avgDriversTrips).Select(x => x.AvarageMileagePerTrip).Average();

                        //records = ModuleData.DriverCollection
                        //    .Where(x => x.LastName.ToLower().Contains(search) && x.CountTrips > avgDriversTrips)
                        //    .OrderByDescending(x => x.GetEffectivityPercent(avgMileage)) //x.TotalMileage/avgMileage
                        //    .Take(CountTake).ToArray();
                        records = baseCollection
                            .Where(x => x.LastName.ToLower().Contains(search) && x.CountTrips > avgDriversTrips)
                            .OrderByDescending(x => x.GetEffectivityPercent(avgMileage)) //x.TotalMileage/avgMileage
                            .Take(CountTake).ToArray();

                        UpdateChartData(records);

                        Formatter = val => (Math.Round(val, 2)).ToString() + " %";
                        Mapper = Mappers.Xy<Driver>()
                    .X((driver, ind) => ind)
                    .Y(driver => Math.Round(driver.GetEffectivityPercent(avgMileage), 2)); //driver.TotalMileage/avgMileage

                        break;

                    case 2:
                        //records = ModuleData.DriverCollection
                        //    .Where(x => x.LastName.ToLower().Contains(search))
                        //    .OrderByDescending(x => x.AvarageMileagePerTrip)
                        //    .Take(CountTake).ToArray();
                        records = baseCollection
                            .Where(x => x.LastName.ToLower().Contains(search))
                            .OrderByDescending(x => x.AvarageMileagePerTrip)
                            .Take(CountTake).ToArray();

                        UpdateChartData(records);

                        Formatter = val => (Math.Round(val, 2)).ToString() + " км/поездка";
                        Mapper = Mappers.Xy<Driver>()
                    .X((driver, ind) => ind)
                    .Y(driver => driver.AvarageMileagePerTrip);
                        break;

                    case 3:
                        //records = ModuleData.DriverCollection
                        //    .Where(x => x.LastName.ToLower().Contains(search))
                        //    .OrderByDescending(x => x.CountTrips)
                        //    .Take(CountTake).ToArray();
                        records = baseCollection
                            .Where(x => x.LastName.ToLower().Contains(search))
                            .OrderByDescending(x => x.CountTrips)
                            .Take(CountTake).ToArray();

                        UpdateChartData(records);

                        Formatter = val => (val).ToString() + " поездок";
                        Mapper = Mappers.Xy<Driver>()
                    .X((driver, ind) => ind)
                    .Y(driver => driver.CountTrips);

                        break;

                    case 4:
                        records = baseCollection
                            .Where(x => x.LastName.ToLower().Contains(search))
                            .OrderByDescending(x => x.MaxTripMileage.Key)
                            .Take(CountTake).ToArray();
                        //records = ModuleData.DriverCollection
                        //    .Where(x => x.LastName.ToLower().Contains(search))
                        //    .OrderByDescending(x => x.MaxTripMileage.Key)
                        //    .Take(CountTake).ToArray();

                        UpdateChartData(records);

                        Formatter = val => (Math.Round(val, 2)).ToString() + " км";
                        Mapper = Mappers.Xy<Driver>()
                    .X((driver, ind) => ind)
                    .Y(driver => driver.MaxTripMileage.Key);

                        break;

                    case 5:
                        //records = ModuleData.DriverCollection
                        //    .Where(x => x.LastName.ToLower().Contains(search))
                        //    .OrderByDescending(x => x.MinTripMileage.Key)
                        //    .Take(CountTake).ToArray();
                        records = baseCollection
                            .Where(x => x.LastName.ToLower().Contains(search))
                            .OrderByDescending(x => x.MinTripMileage.Key)
                            .Take(CountTake).ToArray();

                        UpdateChartData(records);

                        Formatter = val => (Math.Round(val, 2)).ToString() + " км";
                        Mapper = Mappers.Xy<Driver>()
                    .X((driver, ind) => ind)
                    .Y(driver => driver.MinTripMileage.Key);

                        break;

                    case 0:
                    default:
                        //records = ModuleData.DriverCollection
                        //    .Where(x => x.LastName.ToLower().Contains(search))
                        //    .OrderByDescending(x => x.HistoryDrive.Select(tr => tr.Value.Sum(ml => ml.TotalMileage)).FirstOrDefault())
                        //    .Take(CountTake).ToArray();
                        records = baseCollection
                            .Where(x => x.LastName.ToLower().Contains(search))
                            .OrderByDescending(x => x.HistoryDrive.Select(tr => tr.Value.Sum(ml => ml.TotalMileage)).FirstOrDefault())
                            .Take(CountTake).ToArray();

                        UpdateChartData(records);

                        Formatter = val => (val).ToString() + " км";
                        Mapper = Mappers.Xy<Driver>()
                    .X((driver, ind) => ind)
                    .Y(driver => driver.HistoryDrive.Select(z => z.Value.Sum(k => k.TotalMileage)).FirstOrDefault());

                        break;
                }
            }
            catch (Exception)
            { }

            RaisePropertyChanged(nameof(DriversStatisticsSAP));
        }


        private void UpdateChartData(Driver[] records)
        {
            DriversStatisticsSAP.Clear();
            DriversStatisticsSAP.AddRange(records);
            Labels.Clear();
            foreach (var x in records) Labels.Add($"{x.LastName} {x.FirstName}.");
        }


        private void UpdateTotalStatistics()
        {
            RaisePropertyChanged(nameof(TotalMileageAtAll));
            RaisePropertyChanged(nameof(TotalTripsAtAll));
            RaisePropertyChanged(nameof(TotalAvgMileagePerTripAtAll));
            RaisePropertyChanged(nameof(TotalMaxTripAtAll));
            RaisePropertyChanged(nameof(TotalMinTripAtAll));
        }


        private Dictionary<string, List<DateTimePoint>> PrepareData(IEnumerable<TripSAP> trips)
        {
            if (trips == null || !trips.Any())
                return null;

            trips = trips.OrderBy(x => x.DepartureFromGarageDate.Date);

            var datesDict = trips.ToLookup(x => x.DepartureFromGarageDate.Date);
            Dictionary<string, List<DateTimePoint>> resultedCollection = new Dictionary<string, List<DateTimePoint>>()
            {
                { "SAP", new List<DateTimePoint>() }
            };

            var startDate = trips.FirstOrDefault()?.DepartureFromGarageDate.Date;
            if (startDate == null)
                return resultedCollection;

            var endDate = trips.LastOrDefault()?.DepartureFromGarageDate.Date;
            if (endDate == null)
                return resultedCollection;

            var currentDate = startDate.Value;

            for (int i = 0; currentDate <= endDate.Value.AddDays(1);)
            {
                var element = datesDict.ElementAtOrDefault(i);

                if (element == null)
                {
                    if (datesDict.Count() == 1)
                        resultedCollection["SAP"].Add(new DateTimePoint() { Value = double.NaN, DateTime = currentDate });
                    return resultedCollection;
                }
                if (element.Key.Date.Equals(currentDate.Date))
                {
                    resultedCollection["SAP"].Add(new DateTimePoint()
                    {
                        Value = element.Sum(x => x.TotalMileage)
                        ,
                        DateTime = element.Key
                    });

                    i++;
                }
                else
                {
                    resultedCollection["SAP"].Add(new DateTimePoint() { Value = double.NaN, DateTime = currentDate });
                }

                currentDate = currentDate.AddDays(1);
            }
            return resultedCollection;
        }


        private async Task<IEnumerable<T>> GetVehicleInfos<T>(
            ProgressDialogController progress, DatesFromToContext context) where T : IntegratedVehicleInfo
        {
            return await Task.Run(() =>
            {
                try
                {
                    var percentage = 60;

                    progress.SetTitle($"Выборка транспортных средств из системы Wialon");
                    if (percentage < 90)
                        percentage += 3;
                    progress.SetProgress(percentage / 100);

                    int intervalForWialonUnload = 100 - percentage;
                    if (intervalForWialonUnload >= 25)
                        intervalForWialonUnload -= 5;
                    int stepForInterval = (ModuleData.VehiclesNavigate.Count / intervalForWialonUnload) + 1;
                    int index = 0;
                    int indexCurrent = 0;

                    List<T> SimpleDataForReport = new List<T>();

                    foreach (var item in ModuleData.Vehicles)
                    {
                        if (index++ > stepForInterval)
                        {
                            progress.SetProgress(++percentage / 100);
                            index = 0;
                        }

                        indexCurrent++;
                        var vehicle = ModuleData.VehiclesNavigate
                            .FirstOrDefault(x => x.StateNumber.ToLower().Contains(item.StateNumber.ToLower()));

                        TripWialon tripWialon = null;
                        if (vehicle != null)
                        {
                            if (!context.IsWithDetails)
                                tripWialon = _wialonContext.GetCarInfo(vehicle.ID,
                                    context.FromDate,
                                    new DateTime(context.ToDate.Year, context.ToDate.Month, context.ToDate.Day,
                                    23, 59, 59));
                            else
                                tripWialon = _wialonContext.GetCarInfoDetails(vehicle.ID,
                                    context.FromDate,
                                    new DateTime(context.ToDate.Year, context.ToDate.Month, context.ToDate.Day,
                                    23, 59, 59));
                        }

                        //if (tripWialon == null)
                        //{
                        //    _logger.Info($"TripSAP null! Item - {item.ID} ({item.StateNumber}) / From:{context.FromDate} / To {context.ToDate}");
                        //    continue;
                        //}

                        progress.SetMessage($"Осталось проверить: {ModuleData.Vehicles.Count - indexCurrent} транспортных средств\n" +
                            $"Проверка {item.UnitModel}  ({item.StateNumber})\n" +
                            $"Количество поездок за период: {item.Trips?.Count} (SAP)\n" +
                            $"Количество поездок за период: {tripWialon?.CountTrips} (Wialon)\n" +
                            $"Показания одометра за период по системе SAP: {item.TripResulted?.TotalMileage} км\n" +
                            $"Показания одометра за период по системе Wialon: {tripWialon?.Mileage} км\n\n" +
                            $"{((tripWialon?.SpeedViolation != null) ? $"Количество превышений скорости: {tripWialon?.SpeedViolation.Count()}" : $"")}");

                        var integratedVehicle = _container.Resolve<T>();

                        integratedVehicle.StateNumber = item.StateNumber;
                        integratedVehicle.UnitModel = item.UnitModel;
                        integratedVehicle.StructureName = item.StructureName;
                        integratedVehicle.Type = item.Type;

                        integratedVehicle.CountTrips.SAP = item.Trips?.Count ?? 0;
                        integratedVehicle.CountTrips.Wialon = tripWialon?.CountTrips ?? 0;
                        integratedVehicle.IndicatorMileage.SAP = item.TripResulted?.TotalMileage ?? 0;
                        integratedVehicle.IndicatorMileage.Wialon = tripWialon?.Mileage;

                        integratedVehicle.WialonMileageTotal = tripWialon?.TotalMileage;
                        integratedVehicle.CountSpeedViolations = tripWialon?.SpeedViolation?.Count() ?? 0;
                        integratedVehicle.SpeedViolation = tripWialon?.SpeedViolation;

                        if (context.IsWithDetails)
                        {
                            (integratedVehicle as IntegratedVehicleInfoDetails).TripsSAP = item.Trips;
                            (integratedVehicle as IntegratedVehicleInfoDetails).TripsWialon = tripWialon?.Trips;
                        }

                        SimpleDataForReport.Add(integratedVehicle);
                    }

                    return SimpleDataForReport;

                }
                catch (Exception ex)
                {
                    this.Error = new IntegrationSolution.Common.Entities.Error()
                    {
                        IsError = true,
                        ErrorDescription = ex.Message
                    };
                }
                return null;
            });
        }
        #endregion
    }
}
