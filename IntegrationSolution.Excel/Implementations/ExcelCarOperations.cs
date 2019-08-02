using IntegrationSolution.Common.Converters;
using IntegrationSolution.Common.Enums;
using IntegrationSolution.Common.Models;
using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using IntegrationSolution.Excel.Common;
using IntegrationSolution.Excel.Interfaces;
using log4net;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace IntegrationSolution.Excel.Implementations
{
    public class ExcelCarOperations : ExcelBase, ICarOperations
    {
        private Dictionary<string, ExcelCellAddress> _tripsAddress;
        private readonly ILog _logger;


        #region Constructors
        public ExcelCarOperations(ExcelPackage excelPackage, IUnityContainer unityContainer) : base(excelPackage, unityContainer)
        {
            _tripsAddress = new Dictionary<string, ExcelCellAddress>();
            _logger = LogManager.GetLogger(this.GetType());
            
            TryInitializeAll();
        }
        #endregion


        #region Initialize headers
        private bool TryInitializeAll()
        {
            if (InitializeDriverHeaders() && InitializeFuelHeaders()
                && InitializeIndicatorsHeaders() && InitializeMoveDateTimeCheckHeaders())
                return true;
            return false;
        }


        private bool InitializeFuelHeaders()
        {
            var fuelAddress = StaticHelper.GetHeadersAddress(this, _headerNames.DepartureBalanceGas, _headerNames.DepartureBalanceDisel, _headerNames.DepartureBalanceLPG);
            if (fuelAddress?.Count == 0)
                return false;

            fuelAddress = StaticHelper.GetHeadersAddress(this,
                _headerNames.DepartureBalanceGas,
                _headerNames.ReturnBalanceGas,
                _headerNames.ConsumptionGasActual,
                _headerNames.ConsumptionGasNormative,
                _headerNames.ConsumptionGasSavingsOrOverruns,

                _headerNames.DepartureBalanceDisel,
                _headerNames.ReturnBalanceDisel,
                _headerNames.ConsumptionDiselActual,
                _headerNames.ConsumptionDiselNormative,
                _headerNames.ConsumptionDiselSavingsOrOverruns,

                _headerNames.DepartureBalanceLPG,
                _headerNames.ReturnBalanceLPG,
                _headerNames.ConsumptionLPGActual,
                _headerNames.ConsumptionLPGNormative,
                _headerNames.ConsumptionLPGSavingsOrOverruns);

            // 15 - fuel columns
            if (fuelAddress.Any())
            {
                _tripsAddress = _tripsAddress.Concat(fuelAddress).ToDictionary(e => e.Key, e => e.Value);
                return true;
            }

            return false;
        }


        private bool InitializeDriverHeaders()
        {
            var driverAddress = StaticHelper.GetHeadersAddress(this, _headerNames.FullNameOfDriver, _headerNames.NumberOfDriver);
            if (driverAddress?.Count == 0)
                return false;

            _tripsAddress = _tripsAddress.Concat(driverAddress).ToDictionary(e => e.Key, e => e.Value);
            return true;
        }


        private bool InitializeIndicatorsHeaders()
        {
            var driverAddress = StaticHelper.GetHeadersAddress(this,
                _headerNames.DepartureOdometerValue,
                _headerNames.ReturnOdometerValue,
                _headerNames.TotalMileage,
                _headerNames.DepartureMotoHoursIndications,
                _headerNames.ReturnMotoHoursIndications,
                _headerNames.MotoHoursIndicationsAtAll);
            if (driverAddress?.Count == 0)
                return false;

            _tripsAddress = _tripsAddress.Concat(driverAddress).ToDictionary(e => e.Key, e => e.Value);
            return true;
        }


        private bool InitializeMoveDateTimeCheckHeaders()
        {
            var driverAddress = StaticHelper.GetHeadersAddress(this,
                _headerNames.DepartureFromGarageDate,
                _headerNames.DepartureFromGarageTime,
                _headerNames.ReturnToGarageDate,
                _headerNames.ReturnToGarageTime,
                _headerNames.TimeOnDutyAtAll);
            if (driverAddress?.Count == 0)
                return false;

            _tripsAddress = _tripsAddress.Concat(driverAddress).ToDictionary(e => e.Key, e => e.Value);
            return true;
        }
        #endregion


        #region Implementation
        public IEnumerable<IVehicleSAP> GetVehicles()
        {
            ICollection<IVehicleSAP> cars = new List<IVehicleSAP>();
            try
            {
                IDictionary<string, ExcelCellAddress> headers = StaticHelper.GetHeadersAddress(
                    this,
                    _headerNames.TypeOfVehicle,
                    _headerNames.ModelOfVehicle,
                    _headerNames.StateNumber,
                    _headerNames.Departments);

                headers.Add(StaticHelper.GetSameHeadersAddress(this, 
                    _headerNames.PartOfStructureNameForResult).FirstOrDefault());

                for (int row = headers.First().Value.Row + 1; row < this.EndCell.Row; row++)
                {
                    IVehicleSAP vehicle = new Car();
                    foreach (var item in headers)
                    {
                        try
                        {
                            switch (item.Key)
                            {
                                case nameof(_headerNames.TypeOfVehicle):
                                    vehicle.Type = this.CurrentWorkSheet.Cells[row, item.Value.Column].Text;
                                    break;

                                case nameof(_headerNames.ModelOfVehicle):
                                    vehicle.UnitModel = this.CurrentWorkSheet.Cells[row, item.Value.Column].Text;
                                    break;

                                case nameof(_headerNames.StateNumber):
                                    vehicle.StateNumber = this.CurrentWorkSheet.Cells[row, item.Value.Column].Text.ToStateNumber();
                                    break;

                                case nameof(_headerNames.Departments):
                                    vehicle.Department = this.CurrentWorkSheet.Cells[row, item.Value.Column].Text;
                                    break;

                                case nameof(_headerNames.PartOfStructureNameForResult):
                                    vehicle.StructureName = this.CurrentWorkSheet.Cells[row, item.Value.Column].Text;
                                    break;

                                default:
                                    continue;
                            }
                        }
                        catch (Exception ex)
                        { throw ex; }
                    }
                    if (!string.IsNullOrWhiteSpace(vehicle.StateNumber))
                        cars.Add(vehicle);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return cars;
        }


        public bool SetFieldsOfVehicleByAvaliableData(ref IVehicleSAP vehicle)
        {
            if (StaticHelper.GetHeadersAddress(this,
                _headerNames.UnitNumber,
                _headerNames.StateNumber,
                _headerNames.PathListStatus).Count == 0
                || vehicle == null)
                return false;

            vehicle.Trips = GetTripsByStateNumber(vehicle.StateNumber)?.ToList();

            return true;
        }


        public IEnumerable<TripSAP> GetTripsByStateNumber(string StateNumber)
        {
            var rows = StaticHelper.GetRowsWithValue(this, StateNumber, _headerNames.StateNumber);
            if (!rows.Any())
                return null;

            List<TripSAP> result = new List<TripSAP>();
            foreach (var row in rows)
            {
                try
                {
                    TripSAP trip = _container.Resolve<TripSAP>();

                    trip.FuelDictionary = StaticHelper.GetFuelDataByRow(this, row.Row, _tripsAddress).ToDictionary(x => x.Key, y => y.Value);
                    trip.Driver = StaticHelper.GetDriverFromRow(this, row.Row, _tripsAddress);                    

                    #region GetHeaders of indicators
                    // Indicators: odometr, mileage...
                    var headerDepartureOdometerValue = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.DepartureOdometerValue)));
                    var headerReturnOdometerValue = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.ReturnOdometerValue)));
                    var headerTotalMileage = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.TotalMileage)));
                    var headerDepartureMotoHoursIndications = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.DepartureMotoHoursIndications)));
                    var headerReturnMotoHoursIndications = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.ReturnMotoHoursIndications)));
                    var headerMotoHoursIndicationsAtAll = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.MotoHoursIndicationsAtAll)));

                    // Inidicators: date, time
                    var headerDepartureFromGarageDate = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.DepartureFromGarageDate)));
                    var headerDepartureFromGarageTime = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.DepartureFromGarageTime)));
                    var headerReturnToGarageDate = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.ReturnToGarageDate)));
                    var headerReturnToGarageTime = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.ReturnToGarageTime)));
                    var headerTimeOnDutyAtAll = _tripsAddress.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.TimeOnDutyAtAll)));
                    #endregion

                    #region SetValues of car
                    // Set values for odometr
                    if (headerDepartureOdometerValue.Value != null)
                        trip.DepartureOdometerValue = CurrentWorkSheet.Cells[row.Row, headerDepartureOdometerValue.Value.Column].Text.ToDouble();

                    if (headerReturnOdometerValue.Value != null)
                        trip.ReturnOdometerValue = CurrentWorkSheet.Cells[row.Row, headerReturnOdometerValue.Value.Column].Text.ToDouble();

                    if (headerTotalMileage.Value != null)
                        trip.TotalMileage = CurrentWorkSheet.Cells[row.Row, headerTotalMileage.Value.Column].Text.ToDouble();

                    if (headerDepartureMotoHoursIndications.Value != null)
                        trip.DepartureMotoHoursIndications = CurrentWorkSheet.Cells[row.Row, headerDepartureMotoHoursIndications.Value.Column].Text.ToDouble();

                    if (headerReturnMotoHoursIndications.Value != null)
                        trip.ReturnMotoHoursIndications = CurrentWorkSheet.Cells[row.Row, headerReturnMotoHoursIndications.Value.Column].Text.ToDouble();

                    if (headerMotoHoursIndicationsAtAll.Value != null)
                        trip.MotoHoursIndicationsAtAll = CurrentWorkSheet.Cells[row.Row, headerMotoHoursIndicationsAtAll.Value.Column].Text.ToDouble();

                    // Start set values for date and time
                    try
                    {
                        if (headerDepartureFromGarageDate.Value != null)
                            trip.DepartureFromGarageDate = CurrentWorkSheet.Cells[row.Row, headerDepartureFromGarageDate.Value.Column].GetValue<DateTime>();

                        if (headerDepartureFromGarageTime.Value != null)
                        {
                            var time = CurrentWorkSheet.Cells[row.Row, headerDepartureFromGarageTime.Value.Column].GetValue<TimeSpan>();
                            trip.DepartureFromGarageDate = trip.DepartureFromGarageDate.Date
                                .AddHours(time.Hours).AddMinutes(time.Minutes).AddSeconds(time.Seconds);
                        }
                    }
                    catch
                    {
                        _logger.Info("Невозможно преобразовать в дату выезда:" + CurrentWorkSheet.Cells[row.Row, headerDepartureFromGarageDate.Value.Column].Value);
                    }

                    try
                    {
                        if (headerReturnToGarageDate.Value != null)
                            trip.ReturnToGarageDate = CurrentWorkSheet.Cells[row.Row, headerReturnToGarageDate.Value.Column].GetValue<DateTime>();



                        if (headerReturnToGarageTime.Value != null)
                        {
                            var time = CurrentWorkSheet.Cells[row.Row, headerReturnToGarageTime.Value.Column].GetValue<TimeSpan>();
                            trip.ReturnToGarageDate = trip.ReturnToGarageDate.Date
                                .AddHours(time.Hours).AddMinutes(time.Minutes).AddSeconds(time.Seconds);
                        }
                    }
                    catch (Exception)
                    {
                        _logger.Info("Невозможно преобразовать в дату возвращения:" + CurrentWorkSheet.Cells[row.Row, headerReturnToGarageDate.Value.Column].Value);
                    }
                    
                    #endregion
                    result.Add(trip);
                }
                catch (Exception ex)
                { throw ex; }
            }
            return result;
        }


        public void WriteInHeadersAndDataForTotalResult(ICollection<IVehicleSAP> vehicles)
        {
            StaticHelper.WriteVehicleDataAndHeaders(this, vehicles,
                _headerNames.TotalMileageResult,
                _headerNames.TotalJobDoneResult,
                _headerNames.ConsumptionGasActualResult,
                _headerNames.ConsumptionDieselActualResult,
                _headerNames.ConsumptionLPGActualResult,
                _headerNames.TotalCostGas,
                _headerNames.TotalCostDisel,
                _headerNames.TotalCostLPG,
                _headerNames.Amortization,
                _headerNames.DriversFOT,
                _headerNames.TotalCost);
        }


        public void WriteInTotalResultOfEachStructure(ICollection<IVehicleSAP> vehicles)
        {
            var total = GetTotal(vehicles);

            if (total.Count == 0)
                return;

            StaticHelper.WriteSummaryFormula(this, total,
                _headerNames.TotalMileageResult,
                _headerNames.TotalJobDoneResult,
                _headerNames.ConsumptionGasActualResult,
                _headerNames.ConsumptionDieselActualResult,
                _headerNames.ConsumptionLPGActualResult,
                _headerNames.TotalCostGas,
                _headerNames.TotalCostDisel,
                _headerNames.TotalCostLPG,
                _headerNames.Amortization,
                _headerNames.DriversFOT,
                _headerNames.TotalCost);

        }
        #endregion


        #region Helpers
        /// <summary>
        /// Get total values of each Structure (Структурные подразделения)
        /// </summary>
        /// <param name="vehicles"></param>
        /// <returns>Dictionary, where key is Structure, value is TotalIndicators</returns>
        private IDictionary<string, TotalIndicators> GetTotal(ICollection<IVehicleSAP> vehicles)
        {
            var structures = vehicles.ToLookup(x => x.StructureName);
            Dictionary<string, TotalIndicators> summary = new Dictionary<string, TotalIndicators>();

            foreach (var structure in structures)
            {
                TotalIndicators total = new TotalIndicators();

                foreach (var auto in structure)
                {
                    if (auto.TripResulted == null)
                        continue;

                    total.Mileage += auto.TripResulted.TotalMileage;
                    total.MotoJob += auto.TripResulted.MotoHoursIndicationsAtAll;
                    total.Gas += auto.TripResulted.FuelDictionary[FuelEnum.Gas].ConsumptionActual;
                    total.LPG += auto.TripResulted.FuelDictionary[FuelEnum.LPG].ConsumptionActual;
                    total.Disel += auto.TripResulted.FuelDictionary[FuelEnum.Disel].ConsumptionActual;
                }

                var prices = _container.Resolve<FuelPrice>();
                total.GasCost = total.Gas * prices.GasCost;
                total.LPGCost = total.LPG * prices.LPGCost;
                total.DiselCost = total.Disel * prices.DiselCost;

                summary.Add(structure.Key, total);
            }

            return summary;
        }
        #endregion
    }
}
