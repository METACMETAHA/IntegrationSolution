using DialogConstruction.Interfaces;
using Integration.ModuleGUI.Models;
using IntegrationSolution.Common.Enums;
using IntegrationSolution.Common.Models;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Excel.Interfaces;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Events;
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
        public ICollection<IVehicle> Vehicles { get; set; }
        public ICollection<CarWialon> VehiclesNavigate { get; set; }

        // Vehicles from excel which are not found in Wialon
        public ObservableCollection<IVehicle> VehiclesExcelDistinctWialon { get; set; }

        // Vehicles from Wialon which are not found in excel
        public ObservableCollection<CarWialon> VehiclesWialonDistinctExcel { get; set; }

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
                    (this.ModuleData.ExcelMainFile as ICarOperations).WriteInHeadersAndDataForTotalResult(Vehicles);

                    if (percentage < 90)
                        percentage += 9;
                    progress.SetProgress(percentage / 100);

                    (this.ModuleData.ExcelMainFile as ICarOperations).WriteInTotalResultOfEachStructure(Vehicles);

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
                wnd.ShowModalMessageExternal("Успех!", "Результаты успешно сохранены.");

        }


        public DelegateCommand CheckDifferenceOfTotalSpeedCommand { get; private set; }
        protected async void CheckDifference()
        {
            var wnd = (MetroWindow)Application.Current.MainWindow;

            var desicion = await wnd.ShowMessageAsync("Вы хотите продолжить?", "Данная процедура может занять около 5 минут.", MessageDialogStyle.AffirmativeAndNegative);
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
                VehiclesNavigate = wialonCars;


            var datesFromTo = await _dialogManager.ShowDialogAsync<DatesFromToContext>(DialogNamesEnum.DatesFromTo);
            if (datesFromTo == null)
                return;

            var progress = await InitializeCars();

            await Task.Run(() =>
             {
                 this.VehiclesExcelDistinctWialon = new ObservableCollection<IVehicle>(
                     this.Vehicles.Where(x => VehiclesNavigate.FirstOrDefault(wialon => wialon.StateNumber == x.StateNumber) == null));
                 this.VehiclesWialonDistinctExcel = new ObservableCollection<CarWialon>(
                     this.VehiclesNavigate.Where(x => Vehicles.FirstOrDefault(veh => veh.StateNumber == x.StateNumber) == null));
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
                    int stepForInterval = (VehiclesNavigate.Count / intervalForWialonUnload) + 1;
                    int index = 0;
                    int indexCurrent = 0;

                    foreach (var item in VehiclesNavigate)
                    {
                        if (index++ > stepForInterval)
                        {
                            progress.SetProgress(++percentage / 100);
                            index = 0;
                        }

                        indexCurrent++;
                        var vehicle = Vehicles.FirstOrDefault(x => x.StateNumber == item.StateNumber);
                        if (vehicle == null)
                            continue;
                        
                        var tripWialon = _wialonContext.GetCarInfo(item.ID,
                            datesFromTo.FromDate, datesFromTo.ToDate);

                        progress.SetMessage($"Осталось проверить: {VehiclesNavigate.Count - indexCurrent} транспортных средств\n" +
                            $"Проверка {vehicle.UnitModel}  ({vehicle.StateNumber})\n" +
                            $"Количество поездок за период: {vehicle.Trips?.Count} (SAP)\n" +
                            $"Показания одометра за период по системе SAP: {vehicle.TripResulted?.TotalMileage} км\n" +
                            $"Показания одометра за период по системе Wialon: {tripWialon.TotalMileage} км\n\n" +
                            $"{((tripWialon.SpeedViolation != null)? $"Превышения скорости: {tripWialon.SpeedViolation.LocationBegin}\nСкорость фактическая/допустимая: {tripWialon.SpeedViolation.MaxSpeed}/{tripWialon.SpeedViolation.SpeedLimit}\nРасстояние: {tripWialon.SpeedViolation.Mileage} км)" : $"")}");

                        if (vehicle.TripResulted != null) ;
                            //var diff = vehicle.TripResulted.TotalMileage - tripWialon.Mileage;
                        //if(Math.Abs(diff))
                    }

                    percentage = 99;
                    progress.SetProgress(percentage / 100);

                    //(this.ModuleData.ExcelMainFile as ICarOperations).WriteInTotalResultOfEachStructure(Vehicles);
                                        
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
                wnd.ShowModalMessageExternal("Успех!", "Результаты успешно сохранены.");

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

                    Vehicles = cars;
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
