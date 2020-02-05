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
        private static HeaderNames _headerNames;

        [Dependency]
#pragma warning disable IDE1006 // Naming Styles
        public static IUnityContainer container { get; set; }
#pragma warning restore IDE1006 // Naming Styles


        public StaticHelper(IUnityContainer unity)
        {
            container = unity;
            _headerNames = container.Resolve<HeaderNames>();
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
                if (excelFile?.CurrentWorkSheet is null)
                    excelFile.TryOpen();

                foreach (var header in headers)
                {
                    try
                    {
                        var address = (from cell in excelFile.CurrentWorkSheet.Cells[
                            excelFile.StartCell.Row,
                            excelFile.StartCell.Column,
                            excelFile.StartCell.Row + 1,
                            excelFile.CurrentWorkSheet.Dimension.Columns]
                                       where cell.Text.ToLower().Trim() == header.ToLower().Trim()
                                       select cell.Start).First();
                        var propName = _headerNames.PropertiesData.First(x => x.Value == header).Key;
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
                if (excelFile?.CurrentWorkSheet is null)
                    excelFile.TryOpen();

                foreach (var header in headers)
                {
                    try
                    {
                        var address = (from cell in excelFile.CurrentWorkSheet.Cells[
                            excelFile.StartCell.Row,
                            excelFile.StartCell.Column,
                            excelFile.StartCell.Row + 1,
                            excelFile.EndCell.Column]
                                       where cell.Text.ToLower().Contains(header.ToLower())
                                       select cell.Start).First();
                        var propName = _headerNames.PropertiesData.First(x => x.Value == header).Key;
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

                var search = (from cell in excelFile.CurrentWorkSheet.Cells
                               [excelFile.StartCell.Row,
                               address.Column,
                               excelFile.EndCell.Row, address.Column]
                              let cellVal = (header == _headerNames.PropertiesData[nameof(_headerNames.StateNumber)]) ? // checking cell for StateNumber or not (need for convert string)
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

                var search = (from cell in excelFile.CurrentWorkSheet.Cells
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
                //    return GetHeadersAddress(_headerNames.UnitNumber, _headerNames.UnitModel, _headerNames.StateNumber);

                //case Type carType when carType == typeof(IFuel):
                //    return GetHeadersAddress(_headerNames.DepartureBalanceGas, _headerNames.DepartureBalanceDisel, _headerNames.DepartureBalanceLPG);

                //case Type carType when carType == typeof(Gas):
                //    return GetHeadersAddress(_headerNames.ConsumptionGasActual, _headerNames.ConsumptionGasNormative, _headerNames.ConsumptionGasSavingsOrOverruns,
                //        _headerNames.DepartureBalanceGas, _headerNames.ReturnBalanceGas);

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
                //case nameof(_headerNames.FullNameOfDriver):


                default:
                    try
                    {
                        return excelFile.CurrentWorkSheet.Cells[row, header.Value.Column].Text;
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
                if (fuelType.Key == nameof(_headerNames.DepartureBalanceGas)
                    || fuelType.Key == nameof(_headerNames.DepartureBalanceDisel)
                    || fuelType.Key == nameof(_headerNames.DepartureBalanceLPG))
                    fuel.DepartureBalance = excelFile.CurrentWorkSheet.Cells[row, fuelType.Value.Column].Text.ToDouble();

                else if (fuelType.Key == nameof(_headerNames.ReturnBalanceGas)
                    || fuelType.Key == nameof(_headerNames.ReturnBalanceDisel)
                    || fuelType.Key == nameof(_headerNames.ReturnBalanceLPG))
                    fuel.ReturnBalance = excelFile.CurrentWorkSheet.Cells[row, fuelType.Value.Column].Text.ToDouble();

                else if (fuelType.Key == nameof(_headerNames.ConsumptionGasActual)
                    || fuelType.Key == nameof(_headerNames.ConsumptionDiselActual)
                    || fuelType.Key == nameof(_headerNames.ConsumptionLPGActual))
                    fuel.ConsumptionActual = excelFile.CurrentWorkSheet.Cells[row, fuelType.Value.Column].Text.ToDouble();

                else if (fuelType.Key == nameof(_headerNames.ConsumptionGasNormative)
                    || fuelType.Key == nameof(_headerNames.ConsumptionDiselNormative)
                    || fuelType.Key == nameof(_headerNames.ConsumptionLPGNormative))
                    fuel.ConsumptionNormative = excelFile.CurrentWorkSheet.Cells[row, fuelType.Value.Column].Text.ToDouble();

                else if (fuelType.Key == nameof(_headerNames.ConsumptionGasSavingsOrOverruns)
                    || fuelType.Key == nameof(_headerNames.ConsumptionDiselSavingsOrOverruns)
                    || fuelType.Key == nameof(_headerNames.ConsumptionLPGSavingsOrOverruns))
                    fuel.ConsumptionSavingsOrOverruns = excelFile.CurrentWorkSheet.Cells[row, fuelType.Value.Column].Text.ToDouble();
            }

            return fuel;
        }


        public static Driver GetDriverFromRow(ExcelBase excelFile, int row, Dictionary<string, ExcelCellAddress> rangeHeaders)
        {
            if (rangeHeaders.Count == 0)
                return null;

            Driver driver = new Driver();

            #region GetName
            var headerName = rangeHeaders.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.FullNameOfDriver)));
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
            var headerNumber = rangeHeaders.FirstOrDefault(x => x.Key.Contains(nameof(_headerNames.NumberOfDriver)));
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
            var header = GetHeadersAddress(excelFile, _headerNames.PropertiesData[nameof(_headerNames.StateNumber)])?.FirstOrDefault();
            if (header != null)
                headerRow = header.Value.Value.Row;

            int currentCol = excelFile.EndCell.Column;
            while (!string.IsNullOrWhiteSpace(excelFile.CurrentWorkSheet.Cells[headerRow, currentCol].Text))
                currentCol++;

            foreach (var item in rangeHeaders)
            {
                excelFile.ExcelDecorator.AddHeader(excelFile.CurrentWorkSheet, headerRow, currentCol++, item);
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
                var search = _headerNames.PropertiesData.FirstOrDefault(x => x.Key == item.Key).Value;
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
            var headers = GetSameHeadersAddress(excelFile
                , _headerNames.PropertiesData[nameof(_headerNames.PartOfStructureNameForResult)]
                , _headerNames.PropertiesData[nameof(_headerNames.Departments)]);
            if (headers == null || headers.Count != 2)
                return;
            
            var rowsToWrite = GetRowsWithFormula(excelFile, _headerNames.PropertiesData[nameof(_headerNames.Departments)]);

            var headersToFillAddress = GetHeadersAddress(excelFile, HeadersToFill);
            if (headersToFillAddress.Count != HeadersToFill.Length)
                return;

            foreach (var row in rowsToWrite)
            {
                string toFind = excelFile.CurrentWorkSheet.Cells[row.Row, row.Column].Text;
                var data = summary.FirstOrDefault(x => x.Key == toFind);
                if (data.Value == null)
                    continue;

                foreach (var header in headersToFillAddress)
                {
                    if (header.Key == _headerNames.PropertiesData[nameof(_headerNames.TotalMileageResult)])
                        excelFile.CurrentWorkSheet.SetValue(row.Row, header.Value.Column, data.Value.Mileage);

                    else if (header.Key == _headerNames.PropertiesData[nameof(_headerNames.TotalJobDoneResult)])
                        excelFile.CurrentWorkSheet.SetValue(row.Row, header.Value.Column, data.Value.MotoJob);

                    else if (header.Key == _headerNames.PropertiesData[nameof(_headerNames.ConsumptionGasActualResult)])
                        excelFile.CurrentWorkSheet.SetValue(row.Row, header.Value.Column, data.Value.Gas);

                    else if (header.Key == _headerNames.PropertiesData[nameof(_headerNames.ConsumptionDieselActualResult)])
                        excelFile.CurrentWorkSheet.SetValue(row.Row, header.Value.Column, data.Value.Disel);

                    else if (header.Key == _headerNames.PropertiesData[nameof(_headerNames.ConsumptionLPGActualResult)])
                        excelFile.CurrentWorkSheet.SetValue(row.Row, header.Value.Column, data.Value.LPG);

                    else if (header.Key == _headerNames.PropertiesData[nameof(_headerNames.TotalCostGas)])
                        excelFile.CurrentWorkSheet.SetValue(row.Row, header.Value.Column, data.Value.GasCost);

                    else if (header.Key == _headerNames.PropertiesData[nameof(_headerNames.TotalCostDisel)])
                        excelFile.CurrentWorkSheet.SetValue(row.Row, header.Value.Column, data.Value.DiselCost);

                    else if (header.Key == _headerNames.PropertiesData[nameof(_headerNames.TotalCostLPG)])
                        excelFile.CurrentWorkSheet.SetValue(row.Row, header.Value.Column, data.Value.LPGCost);
                }
            }
        }


        private static void AddOrUpdateFuelColumn(ExcelBase excelFile, IVehicleSAP vehicle)
        {
            if (vehicle == null || vehicle.Trips == null || !vehicle.Trips.Any())
                return;

            var header = StaticHelper.GetRowsWithValue(excelFile
                , vehicle.StateNumber
                , _headerNames.PropertiesData[nameof(_headerNames.StateNumber)]);
            if (header == null || !header.Any())
                return;

            var fuelHeaders = StaticHelper.GetHeadersAddress(excelFile
                , _headerNames.PropertiesData[nameof(_headerNames.ConsumptionGasActualResult)]
                , _headerNames.PropertiesData[nameof(_headerNames.ConsumptionDieselActualResult)]
                , _headerNames.PropertiesData[nameof(_headerNames.ConsumptionLPGActualResult)]);
            if (fuelHeaders.Count != 3)
                return;

            foreach (var item in vehicle.TripResulted?.FuelDictionary)
            {
                try
                {
                    switch (item.Key)
                    {
                        case FuelEnum.Disel:
                            excelFile.CurrentWorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(_headerNames.ConsumptionDieselActualResult)].Column,
                                item.Value.ConsumptionActual);
                            break;

                        case FuelEnum.Gas:
                            excelFile.CurrentWorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(_headerNames.ConsumptionGasActualResult)].Column,
                                item.Value.ConsumptionActual);
                            break;

                        case FuelEnum.LPG:
                            excelFile.CurrentWorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(_headerNames.ConsumptionLPGActualResult)].Column,
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

            var header = StaticHelper.GetRowsWithValue(excelFile
                , vehicle.StateNumber
                , _headerNames.PropertiesData[nameof(_headerNames.StateNumber)]);
            if (header == null || !header.Any())
                return;

            var fuelHeaders = StaticHelper.GetHeadersAddress(excelFile
                , _headerNames.PropertiesData[nameof(_headerNames.TotalMileageResult)]
                , _headerNames.PropertiesData[nameof(_headerNames.TotalJobDoneResult)]);
            if (fuelHeaders.Count != 2)
                return;

            try
            {
                excelFile.CurrentWorkSheet.SetValue(
                                    header.FirstOrDefault().Row,
                                    fuelHeaders[nameof(_headerNames.TotalMileageResult)].Column,
                                    vehicle.TripResulted?.TotalMileage);

                excelFile.CurrentWorkSheet.SetValue(
                                    header.FirstOrDefault().Row,
                                    fuelHeaders[nameof(_headerNames.TotalJobDoneResult)].Column,
                                    vehicle.TripResulted?.MotoHoursIndicationsAtAll);
            }
            catch (Exception)
            { }
        }


        private static void AddOrUpdateCostColumn(ExcelBase excelFile, IVehicleSAP vehicle)
        {
            if (vehicle == null || vehicle.Trips == null || !vehicle.Trips.Any())
                return;

            var header = StaticHelper.GetRowsWithValue(excelFile
                , vehicle.StateNumber
                , _headerNames.PropertiesData[nameof(_headerNames.StateNumber)]);
            if (header == null || !header.Any())
                return;

            var fuelHeaders = StaticHelper.GetHeadersAddress(excelFile
                ,_headerNames.PropertiesData[nameof(_headerNames.Amortization)]
                , _headerNames.PropertiesData[nameof(_headerNames.TotalCost)]
                , _headerNames.PropertiesData[nameof(_headerNames.DriversFOT)]
                , _headerNames.PropertiesData[nameof(_headerNames.TotalCostGas)]
                , _headerNames.PropertiesData[nameof(_headerNames.TotalCostDisel)]
                , _headerNames.PropertiesData[nameof(_headerNames.TotalCostLPG)]);
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
                            excelFile.CurrentWorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(_headerNames.TotalCostDisel)].Column,
                                item.Value.ConsumptionActual * price.DiselCost);
                            break;

                        case FuelEnum.Gas:
                            costForFuel += item.Value.ConsumptionActual * price.GasCost;
                            excelFile.CurrentWorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(_headerNames.TotalCostGas)].Column,
                                item.Value.ConsumptionActual * price.GasCost);
                            break;

                        case FuelEnum.LPG:
                            costForFuel += item.Value.ConsumptionActual * price.LPGCost;
                            excelFile.CurrentWorkSheet.SetValue(
                                header.FirstOrDefault().Row,
                                fuelHeaders[nameof(_headerNames.TotalCostLPG)].Column,
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
                excelFile.CurrentWorkSheet.SetValue(
                                    header.FirstOrDefault().Row,
                                    fuelHeaders[nameof(_headerNames.DriversFOT)].Column,
                                    8470);


                var costAtAll = costForFuel + costAmortizationAndDriver * 2;
                excelFile.CurrentWorkSheet.SetValue(
                                    header.FirstOrDefault().Row,
                                    fuelHeaders[nameof(_headerNames.TotalCost)].Column,
                                    costAtAll);
            }
            catch (Exception)
            { }
        }
    }
}
