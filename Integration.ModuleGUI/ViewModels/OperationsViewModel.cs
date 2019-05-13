using DialogConstruction.Interfaces;
using Integration.ModuleGUI.Models;
using Integration.ModuleGUI.Views.OperationResultsViews;
using IntegrationSolution.Common.Enums;
using IntegrationSolution.Common.Models;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using IntegrationSolution.Excel.Interfaces;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;
using WialonBase.Entities;

namespace Integration.ModuleGUI.ViewModels
{
    public class OperationsViewModel : VMLocalBase
    {
        #region Variables
        private readonly IDialogManager _dialogManager;
        #endregion

        #region Properties

        #endregion

        public OperationsViewModel(IDialogManager dialogManager, IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            this.Title = "Операции";
            this.CanGoBack = true;
            WriteTotalStatisticsInFileCommand = new DelegateCommand(WriteTotalStatisticsJob);
            CheckDifferenceOfTotalSpeedCommand = new DelegateCommand(CheckDifference);

            _dialogManager = dialogManager;
        }


        #region Moves next/back
        public override bool MoveBack() => this.CanGoBack;

        public override async Task<bool> MoveNext()
        {
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
                var userDesicion = wnd.ShowModalMessageExternal("Успех!",
                    "Результаты успешно сохранены.\nЖелаете ли просмотреть результаты?",
                    MessageDialogStyle.AffirmativeAndNegative);
                try
                {
                    if (userDesicion == MessageDialogResult.Affirmative)
                        System.Diagnostics.Process.Start(ModuleData.PathToMainFile);
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
        }


        public DelegateCommand CheckDifferenceOfTotalSpeedCommand { get; private set; }
        protected async void CheckDifference()
        {
            var wnd = (MetroWindow)Application.Current.MainWindow;

            var desicion = await wnd.ShowMessageAsync("Вы хотите продолжить?", "Данная процедура может занять некоторое время.", MessageDialogStyle.AffirmativeAndNegative);
            if (desicion != MessageDialogResult.Affirmative)
                return;

            // Get cars from Wialon
            var wialonCars = _wialonContext.GetCarsEnumarable();
            if (wialonCars == null)
            {
                await wnd.ShowMessageAsync("Ошибка!", "Проверьте подключение к навигационной системе Wialon.");
                return;
            }
            else
                ModuleData.VehiclesNavigate = wialonCars;


            var datesFromTo = await _dialogManager.ShowDialogAsync<DatesFromToContext>(DialogNamesEnum.DatesFromTo);
            if (datesFromTo == null)
                return;

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
                        throw new Exception("Внимательней, число должно быь больше от 0.");
                }
                catch (Exception ex)
                {
                    wnd.ShowModalMessageExternal("Ошибка. Повторите ввод.", ex.Message);
                }
            } while (avaliablePercent <= 0);

            Microsoft.Win32.SaveFileDialog fileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Созранение отчёта...",
                CheckPathExists = true,
                DefaultExt = ".xlsx | .xls",
                Filter = "Excel document (.xlsx)|*.xlsx|Excel document (.xls)|*.xls|All files (*.*)|*.*"
            };
            if (fileDialog.ShowDialog() != true)
                return;


            var progress = await InitializeCars();

            await Task.Run(() =>
             {
                 ModuleData.VehiclesExcelDistinctWialon = new ObservableCollection<IVehicle>(
                     ModuleData.Vehicles.Where(x => ModuleData.VehiclesNavigate.FirstOrDefault(wialon => wialon.StateNumber == x.StateNumber) == null));
                 ModuleData.VehiclesWialonDistinctExcel = new ObservableCollection<CarWialon>(
                     ModuleData.VehiclesNavigate.Where(x => ModuleData.Vehicles.FirstOrDefault(veh => veh.StateNumber == x.StateNumber) == null));
             });

            await Task.Run(() =>
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

                    List<IntegratedVehicleInfo> forReport = new List<IntegratedVehicleInfo>();

                    foreach (var item in ModuleData.VehiclesNavigate)
                    {
                        if (index++ > stepForInterval)
                        {
                            progress.SetProgress(++percentage / 100);
                            index = 0;
                        }

                        indexCurrent++;
                        var vehicle = ModuleData.Vehicles.FirstOrDefault(x => x.StateNumber == item.StateNumber);
                        if (vehicle == null)
                            continue;

                        var tripWialon = _wialonContext.GetCarInfo(item.ID,
                            datesFromTo.FromDate, datesFromTo.ToDate);

                        if (tripWialon == null)
                        {
                            _logger.Info($"Trip null! Item - {item.ID} ({item.StateNumber}) / From:{datesFromTo.FromDate} / To {datesFromTo.ToDate}");
                            continue;
                        }

                        progress.SetMessage($"Осталось проверить: {ModuleData.VehiclesNavigate.Count - indexCurrent} транспортных средств\n" +
                            $"Проверка {vehicle.UnitModel}  ({vehicle.StateNumber})\n" +
                            $"Количество поездок за период: {vehicle.Trips?.Count} (SAP)\n" +
                            $"Показания одометра за период по системе SAP: {vehicle.TripResulted?.TotalMileage} км\n" +
                            $"Показания одометра за период по системе Wialon: {tripWialon.Mileage} км\n\n" +
                            $"{((tripWialon.SpeedViolation != null) ? $"Превышения скорости: {tripWialon.SpeedViolation.LocationBegin}\nСкорость фактическая/допустимая: {tripWialon.SpeedViolation.MaxSpeed}/{tripWialon.SpeedViolation.SpeedLimit}\nРасстояние: {tripWialon.SpeedViolation.Mileage} км)" : $"")}");
                        
                        forReport.Add(new IntegratedVehicleInfo()
                        {
                            StateNumber = item.StateNumber,
                            Model = vehicle.UnitModel,
                            CountTrips = vehicle.Trips?.Count ?? 0,
                            SAP_Mileage = vehicle.TripResulted?.TotalMileage ?? 0,
                            Wialon_Mileage = tripWialon.Mileage
                        });
                    }

                    percentage = 99;
                    progress.SetProgress(percentage / 100);

                    this.CanGoNext = true;
                    progress.SetProgress(1);
                    

                    _container.Resolve<IExcelWriter>().CreateReportDiffMileage(fileDialog.FileName, forReport, avaliablePercent);

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
                wnd.ShowModalMessageExternal("Успех!", "Результаты готовы.");
            }
        }
        #endregion


        #region Helpers
        private async Task<ProgressDialogController> InitializeCars()
        {
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

                    ModuleData.Vehicles = cars;
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

            return progress;
        }
        #endregion
    }
}
