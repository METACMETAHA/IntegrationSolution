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

namespace IntegrationSolution.Excel.Common
{
    public class StaticHelper
    {
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
                        var address = (from cell in excelFile.WorkSheet.Cells[
                            excelFile.StartCell.Row,
                            excelFile.StartCell.Column,
                            excelFile.StartCell.Row+1,
                            excelFile.EndCell.Column]
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


                if (search.Count() > 0)
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
                        fuel.DepartureBalance = excelFile.WorkSheet.Cells[row, fuelType.Value.Column].Text;
                        break;

                    case nameof(HeaderNames.ReturnBalanceGas):
                    case nameof(HeaderNames.ReturnBalanceDisel):
                    case nameof(HeaderNames.ReturnBalanceLPG):
                        fuel.ReturnBalance = excelFile.WorkSheet.Cells[row, fuelType.Value.Column].Text;
                        break;

                    case nameof(HeaderNames.ConsumptionGasActual):
                    case nameof(HeaderNames.ConsumptionDiselActual):
                    case nameof(HeaderNames.ConsumptionLPGActual):
                        fuel.ConsumptionActual = excelFile.WorkSheet.Cells[row, fuelType.Value.Column].Text;
                        break;

                    case nameof(HeaderNames.ConsumptionGasNormative):
                    case nameof(HeaderNames.ConsumptionDiselNormative):
                    case nameof(HeaderNames.ConsumptionLPGNormative):
                        fuel.ConsumptionNormative = excelFile.WorkSheet.Cells[row, fuelType.Value.Column].Text;
                        break;

                    case nameof(HeaderNames.ConsumptionGasSavingsOrOverruns):
                    case nameof(HeaderNames.ConsumptionDiselSavingsOrOverruns):
                    case nameof(HeaderNames.ConsumptionLPGSavingsOrOverruns):
                        fuel.ConsumptionSavingsOrOverruns = excelFile.WorkSheet.Cells[row, fuelType.Value.Column].Text;
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
            var headerName = rangeHeaders.Where(x => x.Key.Contains(nameof(HeaderNames.FullNameOfDriver))).FirstOrDefault();
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
            var headerNumber = rangeHeaders.Where(x => x.Key.Contains(nameof(HeaderNames.NumberOfDriver))).FirstOrDefault();
            if (headerNumber.Value != null)
            {
                driver.UnitNumber = StaticHelper.GetValueByHeaderAndRow(excelFile, headerNumber, row).ToString();
            }
            #endregion

            return driver;
        }
    }
}
