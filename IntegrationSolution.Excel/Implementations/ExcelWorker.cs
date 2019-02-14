using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Implementations.Fuel;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Implementations
{
    public class ExcelWorker : IOperations, IDisposable
    {
        public ExcelPackage Excel { get; private set; }
        private ExcelWorkbook _workbook;
        private ExcelWorksheet _worksheet;
        private ExcelCellAddress _startCell;
        private ExcelCellAddress _endCell;


        public ExcelWorker(ExcelPackage excelPackage)
        {
            Excel = excelPackage;
        }


        public void Dispose()
        {
            Excel?.Dispose();
        }


        /// <summary>
        /// This function is trying to open Excel and set the next values: workbook, worksheet, startCell, endCell
        /// </summary>
        public void TryOpen()
        {
            try
            {
                _workbook = Excel.Workbook;
                _worksheet = _workbook.Worksheets.First();

                SetBorders();
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// This function sets borders: startCell, endCell
        /// </summary>
        public void SetBorders()
        {
            if (_worksheet == null)
                TryOpen();

            _startCell = _worksheet.Dimension.Start;
            _endCell = _worksheet.Dimension.End;
        }


        /// <summary>
        /// This function creates IVehicle-objects from excel file.
        /// At the first step, it detect type by T
        /// At the second step, it creates related objects of IVehicle 
        /// and fill its fields by their property names and values in address cells (It works Reflection)
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IEnumerable of IVehicle</returns>
        public IEnumerable<IVehicle> GetVehicle<T>() where T : class, IVehicle
        {
            ICollection<IVehicle> cars = new List<IVehicle>();
            try
            {
                IDictionary<string, ExcelCellAddress> headers = GetHeadersAddress<T>();
                headers = FilterData.CheckHeadersFromObject<T>(headers.Keys).ToIntersectedDictionary(headers);
                

                for (int row = _startCell.Row + 1; row <= _endCell.Row; row++)
                {
                    var vehicle = (T)Activator.CreateInstance(typeof(T));
                    foreach (var header in headers)
                    {
                        vehicle.GetType().InvokeMember(header.Key,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                        Type.DefaultBinder, vehicle, new object[] { _worksheet.Cells[row, header.Value.Column].Text});
                    }
                    vehicle.Fuels = GetFuelByVehicle(row);
                    cars.Add(vehicle);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return cars;
        }


        public IEnumerable<IFuel> GetFuelByVehicle(int row)
        {
            ICollection<IFuel> fuels = new List<IFuel>();
            try
            {
                IDictionary<string, ExcelCellAddress> headers = GetHeadersAddress<IFuel>();
                IEnumerable<Type> fuelTypesOfCurrentVehicle = GetFuelTypes(headers, row);

                foreach (Type fuel in fuelTypesOfCurrentVehicle)
                {
                    headers.Clear();
                    headers = GetHeadersAddress<IFuel>(fuel);

                    foreach (var item in headers)
                    {
                        // получить дабл значения по строке и кеолонке...вынести в отдельный метод
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        

        public IEnumerable<Type> GetFuelTypes(IDictionary<string, ExcelCellAddress> cellsToCheck, int rowOfVehicle)
        {
            ICollection<Type> result = new Collection<Type>();
            foreach (var cell in cellsToCheck)
            {
                try
                {
                    var data = _worksheet.Cells[rowOfVehicle, cell.Value.Column].Text;
                    if (string.IsNullOrWhiteSpace(data))
                        continue;

                    double value = double.Parse(data, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture);

                    if (value == 0)
                        continue;

                    switch (cell.Key)
                    {
                        case nameof(HeaderNames.DepartureBalanceGas):
                            result.Add(typeof(Gas));
                            break;

                        case nameof(HeaderNames.DepartureBalanceDisel):
                            result.Add(typeof(Disel));
                            break;

                        case nameof(HeaderNames.DepartureBalanceLPG):
                            result.Add(typeof(LPG));
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                
            }
            return result;
        }


        /// <summary>
        /// According to the type(T), this fuction get special headers for related object.
        /// If type(T) is not provided for it returns null by default.
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IDictionary of desired cells (where key="name of header", value="address of cell")
        /// else it returns null</returns>
        private IDictionary<string, ExcelCellAddress> GetHeadersAddress<T>(Type type = null) where T : class
        {
            Type checkType = type;
            if(checkType is null)
                checkType = typeof(T);

            switch (checkType)
            {
                case Type carType when carType == typeof(Car):                    
                    return GetHeadersAddress(HeaderNames.UnitNumber, HeaderNames.UnitModel, HeaderNames.StateNumber);

                case Type carType when carType == typeof(IFuel):
                    return GetHeadersAddress(HeaderNames.DepartureBalanceGas, HeaderNames.DepartureBalanceDisel, HeaderNames.DepartureBalanceLPG);

                case Type carType when carType == typeof(Gas):
                    return GetHeadersAddress(HeaderNames.ConsumptionGasActual, HeaderNames.ConsumptionGasNormative, HeaderNames.ConsumptionGasSavingsOrOverruns,
                        HeaderNames.DepartureBalanceGas, HeaderNames.ReturnBalanceGas);

                default:
                    return null;
            }
        }


        /// <summary>
        /// This function gets headers` names and find them in excel headers (first row).
        /// At the same time it replaces name of headers to related property name.
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IDictionary where key="name of header`s property", value="address of cell"</returns>
        private IDictionary<string, ExcelCellAddress> GetHeadersAddress(params string[] headers)
        {
            IDictionary<string, ExcelCellAddress> headersCells = new Dictionary<string, ExcelCellAddress>();
            try
            {
                if (_worksheet is null)
                    TryOpen();

                foreach (var header in headers)
                {
                    try
                    {
                        var address = (from cell in _worksheet.Cells[_startCell.Row, _startCell.Column, _startCell.Row, _endCell.Column]
                                    where cell.Text == header
                                    select cell.Start).First();
                        var propName = HeaderNames.PropertiesData.First(x => x.Value == header).Key;
                        headersCells.Add(propName, address);
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
        

        private void GetStateNumber()
        {

        }


    }
}
