using DialogConstruction.Interfaces;
using Integration.ModuleGUI.Models;
using IntegrationSolution.Common.Enums;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Common.Models;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using IntegrationSolution.Excel.Interfaces;
using LiveCharts;
using LiveCharts.Configurations;
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
            set { _commonCars = value; }
        }

        private Driver selectedDriverChart;
        public Driver SelectedDriverChart
        {
            get { return selectedDriverChart; }
            set { SetProperty(ref selectedDriverChart, value); }
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
                //RaisePropertyChanged(nameof(DriversFilteredList));
            }
        }

        private ChartValues<Driver> driversStatisticsSAP;
        public ChartValues<Driver> DriversStatisticsSAP
        {
            get { return driversStatisticsSAP; }
            set { SetProperty(ref driversStatisticsSAP, value); }
        }

        public object Mapper { get; set; }

        // Collection with filter
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
                    /*x.HistoryDrive.Keys.Select(key => key.StateNumber.Contains(SearchField)*/
                    
                }
                //if (IsHideNullMileageCars == true)
                //    dataList = dataList?.Where(x => x.PercentDifference != null)?.ToList();

                //if (IsShowOnlyViolationsCars == true)
                //    dataList = dataList?.Where(x => x.CountSpeedViolations > 0)?.ToList();

                if (dataList == null)
                    return new ObservableCollection<Driver>();

                return new ObservableCollection<Driver>(dataList);
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
            GridConfiguration = new GridConfiguration();

            _dialogManager = dialogManager;

            Mapper = Mappers.Xy<Driver>()
                .X((driver, index) => index)
                .Y(driver => driver.HistoryDrive.Keys.Count);
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
                            ModuleData.SimpleDataForReport.OrderBy(x => x.PercentDifference).ToList(), avaliablePercent,
                            ModuleData.VehiclesExcelDistinctWialon.ToList(),
                            ModuleData.VehiclesWialonDistinctExcel.ToList());
                    else
                        _container.Resolve<IExcelWriter>().CreateReportDiffMileageWithDetails(fileDialog.FileName,
                            ModuleData.DetailsDataForReport.OrderBy(x => x.PercentDifference).ToList(), avaliablePercent,
                            ModuleData.VehiclesExcelDistinctWialon.ToList(),
                            ModuleData.VehiclesWialonDistinctExcel.ToList());

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
            var car = ModuleData.Vehicles.Where(x => chartPoint.SeriesView.Title.Contains(x.StateNumber)).FirstOrDefault();
            if (car == null)
                return;

            StringBuilder msg = new StringBuilder();
            if(!string.IsNullOrWhiteSpace(car.Department))
                msg.AppendLine($"Служба/отдел:\t{car.Department}");

            if (!string.IsNullOrWhiteSpace(car.StructureName))
                msg.AppendLine($"Структурное подразделение:\t{car.StructureName}");

            if (car.TripResulted != null)
            {
                msg.AppendLine($"Количество выездов:\t{car.CountTrips}");
                msg.AppendLine($"Пробег за период:\t{car.TripResulted.TotalMileage} км");
                msg.AppendLine($"Средний пробег за поездку:\t{Math.Round((car.TripResulted.TotalMileage/car.CountTrips.Value), 2)} км/поездка");
            }


            await _dialogManager.ShowMessageBox($"{car.UnitModel}\t{car.StateNumber}", msg.ToString());
        }


        public DelegateCommand OpenDriversCarsPopupCommand { get; private set; }
        private void OpenDriversCarsPopup()
        {
            this.IsDriversCarsPopupVisible = !IsDriversCarsPopupVisible;
        }


        public DelegateCommand OnDriverChangedCmd { get; private set; }
        [STAThreadAttribute]
        protected async void OnDriverChanged()
        {
            if (SelectedDriverChart == null)
                return;

            if (SelectedDriverChart.HistoryDrive != null)
            {
                await Task.Run(() => {
                   // SelectedDriverChart.HistoryDrive.Values.Count
                });
            }
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
                var MileageStatisticsSAP = ModuleData.Vehicles.OrderByDescending(x => x?.TripResulted?.TotalMileage).Take(10);
                var AverageMileageByTripStatisticsSAP = ModuleData.Vehicles.OrderByDescending(x => (x?.TripResulted?.TotalMileage / x.CountTrips ?? -1)).Take(10);

                CommonCars = new ObservableCollection<IVehicleSAP>(MileageStatisticsSAP.Intersect(AverageMileageByTripStatisticsSAP));

                CarMileageStatisticsSAP = InitializeChartsData(MileageStatisticsSAP
                    , ChartDefinition.CarMileageStatisticsSAP
                    , chartPoint => string.Format("{0} км", chartPoint.Y));
                
                CarAverageMileageByTripStatisticsSAP = InitializeChartsData(AverageMileageByTripStatisticsSAP
                    , ChartDefinition.CarAverageMileageByTripStatisticsSAP
                    , chartPoint => string.Format("{0} км", chartPoint.Y));

                // Initialize Drivers
                InitializeDrivers(ModuleData.Vehicles.AsEnumerable());
            }

            return progress;
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
                                car_atCollection.Value.Add(item.Value);
                            }
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

                // IComparable for drivers and after that set : Results = records.AsChartValues();
                var record = ModuleData.DriverCollection.Select(x => x.HistoryDrive.Select(tr => tr.Value.Sum(ml => ml.TotalMileage))).ToList();


                RaisePropertyChanged(nameof(DriversFilteredList));

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
