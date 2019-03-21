using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Excel.Implementations;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Common
{
    public class CommonOperationsBase
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
                                       where cell.Text.Trim().ToLower() == header.ToLower()
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

    }
}
