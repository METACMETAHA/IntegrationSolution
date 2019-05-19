using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Excel.Implementations;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using IntegrationSolution.Common.Converters;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Common.Enums;
using IntegrationSolution.Entities.SelfEntities;
using IntegrationSolution.Common.Models;
using Unity;
using Unity.Attributes;

namespace IntegrationSolution.Excel.Common
{
    public class StaticHelper
    {
        [Dependency]
#pragma warning disable IDE1006 // Naming Styles
        public static IUnityContainer container { get; set; }
#pragma warning restore IDE1006 // Naming Styles


        public StaticHelper(IUnityContainer unity)
        {
            container = unity;
        }
        

        /// <summary>
        /// This function gets headers` names and find them in excel headers (first row).
        /// At the same time it replaces name of headers to related property name.
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IDictionary where key="name of header`s property", value="address of cell"</returns>
        public static IDictionary<string, ExcelCellAddress> GetHeadersAddress(ExcelBase excelFile, params string[] headers)
        {
            IDictionary<string, ExcelCellAddress> headersCells = new Dictionary<string, ExcelCellAddress>();
            try
            {
                if (excelFile?.WorkSheet is null)
                    excelFile.TryOpen();

                foreach (var header in headers)
                {
                    try
                    {
                        var ss = excelFile.WorkSheet.Cells[
                            excelFile.StartCell.Row,
                            excelFile.StartCell.Column,
                            excelFile.StartCell.Row + 1,
                            excelFile.WorkSheet.Dimension.Columns];
                        var address = (from cell in excelFile.WorkSheet.Cells[
                            excelFile.StartCell.Row,
                            excelFile.StartCell.Column,
                            excelFile.StartCell.Row + 1,
                            excelFile.WorkSheet.Dimension.Columns]
                                       where cell.Text.ToLower() == header.ToLower()
                                       select cell.Start).First();
                        var propName = HeaderNames.PropertiesData.First(x => x.Value == header).Key;
                        headersCells.Add(propName, address);

                        if (headersCells.Count == headers.Length)
                            break;
                    }
                    catch (Exception)
                    { }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return headersCells;
        }


        /// <summary>
        /// This function gets headers` names and find the same in excel headers (first row).
        /// At the same time it replaces name of headers to related property name.
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IDictionary where key="name of header`s property", value="address of cell"</returns>
        public static IDictionary<string, ExcelCellAddress> GetSameHeadersAddress(ExcelBase excelFile, params string[] headers)
        {
            IDictionary<string, ExcelCellAddress> headersCells = new Dictionary<string, ExcelCellAddress>();
            try
            {
                if (excelFile?.WorkSheet is null)
                    excelFile.TryOpen();

                foreach (var header in headers)
                {
                    try
                    {
                        var address = (from cell in excelFile.WorkSheet.Cells[
                            excelFile.StartCell.Row,
                            excelFile.StartCell.Column,
                            excelFile.StartCell.Row + 1,
                            excelFile.EndCell.Column]
                                       where cell.Text.ToLower().Contains(header.ToLower())
                                       select cell.Start).First();
                        var propName = HeaderNames.PropertiesData.First(x => x.Value == header).Key;
                        headersCells.Add(propName, address);

                        if (headersCells.Count == headers.Length)
                            break;
                    }
                    catch (Exception)
                    { }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return headersCells;
        }


        public static IEnumerable<ExcelCellAddress> GetRowsWithValue(ExcelBase excelFile, string Value, params string[] searchingHeaders)
        {
            if (string.IsNullOrWhiteSpace(Value) || searchingHeaders.Length == 0 || excelFile == null)
                return null;

            List<ExcelCellAddress> SearchingCells = new List<ExcelCellAddress>();

            foreach (var header in searchingHeaders)
            {
                var address = StaticHelper.GetHeadersAddress(excelFile, header).FirstOrDefault().Value;
                if (address == null)
                    continue;

                var search = (from cell in excelFile.WorkSheet.Cells
                               [excelFile.StartCell.Row,
                               address.Column,
                               excelFile.EndCell.Row, address.Column]
                              let cellVal = (header == HeaderNames.StateNumber) ? // checking cell for StateNumber or not (need for convert string)
                              cell.Text.ToLower().ToStateNumber() : cell.Text.ToLower()
                              where cellVal == Value.ToLower().ToStateNumber()
                              select cell.Start).ToList();


                if (search.Any())
                    SearchingCells.AddRange(search);
            }

            return SearchingCells;
        }


        public static IEnumerable<ExcelCellAddress> GetRowsWithFormula(ExcelBase excelFile, params string[] searchingHeaders)
        {
            if (searchingHeaders.Length == 0 || excelFile == null)
                return null;

            List<ExcelCellAddress> SearchingCells = new List<ExcelCellAddress>();

            foreach (var header in searchingHeaders)
            {
                var address = StaticHelper.GetHeadersAddress(excelFile, header).FirstOrDefault().Value;
                if (address == null)
                    continue;

                var search = (from cell in excelFile.WorkSheet.Cells
                               [excelFile.StartCell.Row,
                               address.Column,
                               excelFile.EndCell.Row, address.Column]
                              where !string.IsNullOrWhiteSpace(cell.Formula)
                              select cell.Start).ToList();


                if (search.Any())
                    SearchingCells.AddRange(search);
            }

            return SearchingCells;
        }


        /// <summary>
        /// According to the type(T), this fuction get special headers for related object.
        /// If type(T) is not provided for it returns null by default.
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IDictionary of desired cells (where key="name of header", value="address of cell")
        /// else it returns null</returns>
        public static IDictionary<string, ExcelCellAddress> GetHeadersAddress<T>(Type type = null) where T : class
        {
            Type checkType = type;
            if (checkType is null)
                checkType = typeof(T);

            switch (checkType)
            {
                //case Type carType when carType == typeof(Car):
                //    return GetHeadersAddress(HeaderNames.UnitNumber, HeaderNames.UnitModel, HeaderNames.StateNumber);

                //case Type carType when carType == typeof(IFuel):
                //    return GetHeadersAddress(HeaderNames.DepartureBalanceGas, HeaderNames.DepartureBalanceDisel, HeaderNames.DepartureBalanceLPG);

                //case Type carType when carType == typeof(Gas):
                //    return GetHeadersAddress(HeaderNames.ConsumptionGasActual, HeaderNames.ConsumptionGasNormative, HeaderNames.ConsumptionGasSavingsOrOverruns,
                //        HeaderNames.DepartureBalanceGas, HeaderNames.ReturnBalanceGas);

                default:
                    return null;
            }
        }


        /// <summary>
        /// For parsing values to double, int or other types
        /// </summary>
        /// <param name="excelFile"></param>
        /// <param name="header"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static object GetValueByHeaderAndRow(ExcelBase excelFile, KeyValuePair<string, ExcelCellAddress> header, int row)
        {
            switch (header.Key)
            {
                //case nameof(HeaderNames.FullNameOfDriver):


                default:
                    try
                    {
                        return excelFile.WorkSheet.Cells[row, header.Value.Column].Text;
                    }
                    catch
                    {
                        return null;
                    }
            }

        }


        public static IDictionary<FuelEnum, IFuel> GetFuelDataByRow(ExcelBase excelFile, int row, Dictionary<string, ExcelCellAddress> rangeHeaders)
        {
            if (rangeHeaders.Count == 0)
                return null;

            Dictionary<FuelEnum, IFuel> forReturn = new Dictionary<FuelEnum, IFuel>();

            foreach (var fuelType in Enum.GetValues(typeof(FuelEnum)).Cast<FuelEnum>())
            {
                var fuelData = StaticHelper.GetSingleFuelDataByRow(excelFile, row, fuelType, rangeHeaders);
                forReturn.Add(fuelType, fuelData);
            }

            return forReturn;
        }


        public static IFuel GetSingleFuelDataByRow(ExcelBase excelFile, int row, FuelEnum Fuel, Dictionary<string, ExcelCellAddress> rangeHeaders)
        {
            if (rangeHeaders.Count == 0)
                return null;

            IFuel fuel = new FuelBase(Fuel.ToString());
            var searchList = rangeHeaders.Where(x => x.Key.Contains(Fuel.ToString())).ToList();

            foreach (var fuelType in searchList)
            {
                switch (fuelType.Key)
                {
                    case nameof(HeaderNames.DepartureBalanceGas):
                    case nameof(HeaderNames.DepartureBalanceDisel):
                    case nameof(HeaderNames.DepartureBalanceLPG):
                        fuel.DepartureBalance = excelFile.WorkSheet.Cells[row, fuelType.Value.Column].Text.ToDouble();
                        break;

                    case nameof(HeaderNames.ReturnBalanceGas):
                    case nameof(HeaderNames.ReturnBalanceDisel):
                    case nameof(HeaderNames.ReturnBalanceLPG):
                        fuel.ReturnBalance = excelFile.WorkSheet.Cells[row, fuelType.Value.Column].Text.ToDouble();
                        break;

                    case nameof(HeaderNames.ConsumptionGasActual):
                    case nameof(HeaderNames.ConsumptionDiselActual):
                    case nameof(HeaderNames.ConsumptionLPGActual):
                        fuel.ConsumptionActual = excelFile.WorkSheet.Cells[row, fuelType.Value.Column].Text.ToDouble();
                        break;

                    case nameof(HeaderNames.ConsumptionGasNormative):
                    case nameof(HeaderNames.ConsumptionDiselNormative):
                    case nameof(HeaderNames.ConsumptionLPGNormative):
                        fuel.ConsumptionNormative = excelFile.WorkSheet.Cells[row, fuelType.Value.Column].Text.ToDouble();
                        break;

                    case nameof(HeaderNames.ConsumptionGasSavingsOrOverruns):
                    case nameof(HeaderNames.ConsumptionDiselSavingsOrOverruns):
                    case nameof(HeaderNames.ConsumptionLPGSavingsOrOverruns):
                        fuel.ConsumptionSavingsOrOverruns = excelFile.WorkSheet.Cells[row, fuelType.Value.Column].Text.ToDouble();
                        break;

                    default:
                        break;
                }
            }

            return fuel;
        }


        public static Driver GetDriverFromRow(ExcelBase excelFile, int row, Dictionary<string, ExcelCellAddress> rangeHeaders)
        {
            if (rangeHeaders.Count == 0)
                return null;

            Driver driver = new Driver();

            #region GetName
            var headerName = rangeHeaders.FirstOrDefault(x => x.Key.Contains(nameof(HeaderNames.FullNameOfDriver)));
            if (headerName.Value != null)
            {
                var name = GetValueByHeaderAndRow(excelFile, headerName, row).ToString().Split(' ');
                if (name.Length == 3)
                {
                    driver.LastName = name.First();
                    driver.FirstName = name.ElementAt(1);
                    driver.Patronymic = name.Last();
                }
            }
            #endregion

            #region GetNumber
            var headerNumber = rangeHeaders.FirstOrDefault(x => x.Key.Contains(nameof(HeaderNames.NumberOfDriver)));
            if (headerNumber.Value != null)
            {
                driver.UnitNumber = StaticHelper.GetValueByHeaderAndRow(excelFile, headerNumber, row).ToString();
            }
            #endregion

            return driver;
        }


        /// <summary>
        /// Excel file should contain StateNumber header
        /// </summary>
        /// <param name="excelFile"></param>
        /// <param name="rangeHeaders"></param>
        public static void AddHeaders(ExcelBase excelFile, params string[] rangeHeaders)
        {
            int headerRow = 0;
            var header = GetHeadersAddress(excelFile, HeaderNames.StateNumber)?.FirstOrDefault();
            if (header != null)
                headerRow = header.Value.Value.Row;

            int currentCol = excelFile.EndCell.Column;
            while (!string.IsNullOrWhiteSpace(excelFile.WorkSheet.Cells[headerRow, currentCol].Text))
                currentCol++;

            foreach (var item in rangeHeaders)
            {
                excelFile.ExcelDecorator.AddHeader(excelFile.WorkSheet, headerRow, currentCol++, item);
            }
        }


        /// <summary>
        /// Append columns of input all data
        /// </summary>
        /// <param name="excelFile">File to write</param>
        /// <param name="vehicles">Collection of cars and their data</param>
        /// <param name="rangeHeaders">Headers to add and fill</param>
        public static void WriteVehicleDataAndHeaders(ExcelBase excelFile, IEnumerable<IVehicleSAP> vehicles, params string[] rangeHeaders)
        {
            var price = container.Resolve<FuelPrice>(); // ?? new FuelPrice() { DiselCost = 10, GasCost = 10, LPGCost = 10 };
            var existedHeaders = GetHeadersAddress(excelFile, rangeHeaders);
            foreach (var item in existedHeaders)
            {
                var search = HeaderNames.PropertiesData.FirstOrDefault(x => x.Key == item.Key).Value;
                if (!string.IsNullOrWhiteSpace(search))
                    rangeHeaders = Array.FindAll(rangeHeaders, x => x != search).ToArray();
            }
            rangeHeaders = Array.FindAll(rangeHeaders, x => !existedHeaders.ContainsKey(x)).ToArray();

            AddHeaders(excelFile, rangeHeaders);

            foreach (var vehicle in vehicles)
            {
                AddOrUpdateMileageColumn(excelFile, vehicle);
                AddOrUpdateFuelColumn(excelFile, vehicle);
                AddOrUpdateCostColumn(excelFile, vehicle);
            }
        }


        /// <summary>
        /// Calculate in program and write down results
        /// </summary>
        /// <param name="excelFile"></param>
        public static void WriteSummaryFormula(ExcelBase excelFile, IDictionary<string, TotalIndicators> summary, params string[] HeadersToFill)
        {
            var headers = GetSameHeadersAddress(excelFile, HeaderNames.PartOfStructureNameForResult, HeaderNames.Departments);
            if (headers == null || headers.Count != 2)
                return;
            
            var rowsToWrite = GetRowsWithFormula(excelFile, HeaderNames.Departments);

            var headersToFillAddress = GetHeadersAddress(excelFile, HeadersToFill);
            if (headersToFillAddress.Count != HeadersToFill.Length)
                return;

            foreach (var row in rowsToWrite)
            {
                string toFind = excelFile.WorkSheet.Cells[row.Row, row.Column].Text;
                var data = summary.FirstOrDefault(x => x.Key == toFind);
                if (data.Value == null)
                    continue;

                foreach (var header in headersToFillAddress)
                {
                    switch (header.Key)
                    {
                        case nameof(HeaderNames.TotalMileageResult):
                            excelFile.WorkSheet.SetValue(row.Row, header.Value.Column, data.Value.Mileage);                          
                            break;
                        case nameof(HeaderNames.TotalJobDoneResult):
                            excelFile.WorkSheet.SetValue(row.Row, header.Value.Column, data.Value.MotoJob);
                            break;
                        case nameof(HeaderNames.ConsumptionGasActualResult):
                            excelFile.WorkSheet.SetValue(row.Row, header.Value.Column, data.Value.Gas);
                            break;
                        case nameof(HeaderNames.ConsumptionDieselActualResult):
                            excelFile.WorkSheet.SetValue(row.Row, header.Value.Column, data.Value.Disel);
                            break;
                        case nameof(HeaderNames.ConsumptionLPGActualResult):
                            excelFile.WorkSheet.SetValue(row.Row, header.Value.Column, data.Value.LPG);
                            break;
                        case nameof(HeaderNames.TotalCostGas):
                            excelFile.WorkSheet.SetValue(row.Row, header.Value.Column, data.Value.GasCost);
                            break;
                        case nameof(HeaderNames.TotalCostDisel):
                            excelFile.WorkSheet.SetValue(row.Row, header.Value.Column, data.Value.DiselCost);
                            break;
                        case nameof(HeaderNames.TotalCostLPG):
                            excelFile.WorkSheet.SetValue(row.Row, header.Value.Column, data.Value.LPGCost);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private static void AddOrUpdateFuelColumn(ExcelBase excelFile, IVehicleSAP vehicle)
        {
            if (vehicle == null || vehicle.Trips == null || !vehicle.Trips.Any())
                return;

            var header = StaticHelper.GetRowsWithValue(excelFile, vehicle.StateNumber, HeaderNames.StateNumber);
            if (header == null || !header.Any())
                return;

            var fuelHeaders = StaticHelper.GetHeadersAddress(excelFile, HeaderNames.ConsumptionGasActualResult,
                HeaderNames.ConsumptionDieselActualResult, HeaderNames.ConsumptionLPGActualResult);
            if (fuelHeaders.Count != 3)
                return;

            foreach (var item in vehicle.TripResulted?.FuelDictionary)
            {
                try
                {
                    switch (item.Key)
                    {
                        case FuelEnum.Disel:
                            excelFile.WorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(HeaderNames.ConsumptionDieselActualResult)].Column,
                                item.Value.ConsumptionActual);
                            break;

                        case FuelEnum.Gas:
                            excelFile.WorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(HeaderNames.ConsumptionGasActualResult)].Column,
                                item.Value.ConsumptionActual);
                            break;

                        case FuelEnum.LPG:
                            excelFile.WorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(HeaderNames.ConsumptionLPGActualResult)].Column,
                                item.Value.ConsumptionActual);
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception)
                { }
            }

        }


        private static void AddOrUpdateMileageColumn(ExcelBase excelFile, IVehicleSAP vehicle)
        {
            if (vehicle == null || vehicle.Trips == null || !vehicle.Trips.Any())
                return;

            var header = StaticHelper.GetRowsWithValue(excelFile, vehicle.StateNumber, HeaderNames.StateNumber);
            if (header == null || !header.Any())
                return;

            var fuelHeaders = StaticHelper.GetHeadersAddress(excelFile, HeaderNames.TotalMileageResult,
                HeaderNames.TotalJobDoneResult);
            if (fuelHeaders.Count != 2)
                return;

            try
            {
                excelFile.WorkSheet.SetValue(
                                    header.FirstOrDefault().Row,
                                    fuelHeaders[nameof(HeaderNames.TotalMileageResult)].Column,
                                    vehicle.TripResulted?.TotalMileage);

                excelFile.WorkSheet.SetValue(
                                    header.FirstOrDefault().Row,
                                    fuelHeaders[nameof(HeaderNames.TotalJobDoneResult)].Column,
                                    vehicle.TripResulted?.MotoHoursIndicationsAtAll);
            }
            catch (Exception)
            {
            }
        }


        private static void AddOrUpdateCostColumn(ExcelBase excelFile, IVehicleSAP vehicle)
        {
            if (vehicle == null || vehicle.Trips == null || !vehicle.Trips.Any())
                return;

            var header = StaticHelper.GetRowsWithValue(excelFile, vehicle.StateNumber, HeaderNames.StateNumber);
            if (header == null || !header.Any())
                return;

            var fuelHeaders = StaticHelper.GetHeadersAddress(excelFile, 
                HeaderNames.Amortization, HeaderNames.TotalCost, HeaderNames.DriversFOT, 
                HeaderNames.TotalCostGas, HeaderNames.TotalCostDisel, HeaderNames.TotalCostLPG);
            if (fuelHeaders.Count != 6)
                return;

            var price = container.Resolve<FuelPrice>();
            double costForFuel = 0;
            foreach (var item in vehicle.TripResulted?.FuelDictionary)
            {
                try
                {
                    switch (item.Key)
                    {
                        case FuelEnum.Disel:
                            costForFuel += item.Value.ConsumptionActual * price.DiselCost;
                            excelFile.WorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(HeaderNames.TotalCostDisel)].Column,
                                item.Value.ConsumptionActual * price.DiselCost);
                            break;

                        case FuelEnum.Gas:
                            costForFuel += item.Value.ConsumptionActual * price.GasCost;
                            excelFile.WorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(HeaderNames.TotalCostGas)].Column,
                                item.Value.ConsumptionActual * price.GasCost);
                            break;

                        case FuelEnum.LPG:
                            costForFuel += item.Value.ConsumptionActual * price.LPGCost;
                            excelFile.WorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(HeaderNames.TotalCostLPG)].Column,
                                item.Value.ConsumptionActual * price.LPGCost);
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception)
                { }
            }
            
            try
            {
                double costAmortizationAndDriver = 8470;
                excelFile.WorkSheet.SetValue(
                                    header.FirstOrDefault().Row,
                                    fuelHeaders[nameof(HeaderNames.DriversFOT)].Column,
                                    8470);


                var costAtAll = costForFuel + costAmortizationAndDriver * 2;
                excelFile.WorkSheet.SetValue(
                                    header.FirstOrDefault().Row,
                                    fuelHeaders[nameof(HeaderNames.TotalCost)].Column,
                                    costAtAll);
            }
            catch (Exception)
            { }
        }
    }
}
