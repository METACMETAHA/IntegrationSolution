using DialogConstruction.Interfaces;
using Integration.ModuleGUI.Models;
using IntegrationSolution.Common.Enums;
using IntegrationSolution.Common.Models;
using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using IntegrationSolution.Excel.Interfaces;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;

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
            this.CanGoNext = false; 
            WriteTotalStatisticsInFileCommand = new DelegateCommand(WriteTotalStatisticsJob);
            CheckDifferenceOfTotalSpeedCommand = new DelegateCommand(CheckDifference);

            _dialogManager = dialogManager;
        }


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

            // Get cars from Wialon
            var wialonCars = _wialonContext.GetCarsEnumarable();
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
                    IEnumerable<IntegratedVehicleInfo> forReport = null;
                    IEnumerable<IntegratedVehicleInfoDetails> forReportDetails = null;

                    if (!datesFromToContext.IsWithDetails)
                    {
                        forReport = await this.GetVehicleInfos<IntegratedVehicleInfo>(progress, datesFromToContext);
                        if(forReport == null || !forReport.Any())
                            throw new Exception("Данные отсутствуют.\nПопробуйте выбрать другой период или повторите попытку позже.");
                    }
                    else
                    {
                        forReportDetails = await this.GetVehicleInfos<IntegratedVehicleInfoDetails>(progress, datesFromToContext);
                        if (forReportDetails == null || !forReportDetails.Any())
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
                            forReport.OrderBy(x => x.PercentDifference).ToList(), avaliablePercent,
                            ModuleData.VehiclesExcelDistinctWialon.ToList(),
                            ModuleData.VehiclesWialonDistinctExcel.ToList());
                    else
                        _container.Resolve<IExcelWriter>().CreateReportDiffMileageWithDetails(fileDialog.FileName,
                            forReportDetails.OrderBy(x => x.PercentDifference).ToList(), avaliablePercent,
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

            return progress;
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

                    List<T> forReport = new List<T>();

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
                                    context.FromDate, context.ToDate);
                            else
                                tripWialon = _wialonContext.GetCarInfoDetails(vehicle.ID,
                                    context.FromDate, context.ToDate);
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

                        if (context.IsWithDetails)
                        {
                            (integratedVehicle as IntegratedVehicleInfoDetails).TripsSAP = item.Trips;
                            (integratedVehicle as IntegratedVehicleInfoDetails).TripsWialon = tripWialon?.Trips;
                        }

                        forReport.Add(integratedVehicle);
                    }

                    return forReport;

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
