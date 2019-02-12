using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IEnumerable of IVehicle</returns>
        public IEnumerable<IVehicle> GetVehicle<T>() where T : class, IVehicle
        {
            ICollection<IVehicle> cars = new List<IVehicle>();
            try
            {
                IDictionary<string, ExcelCellAddress> headers = GetHeadersAddress<T>();
                for (int row = _startCell.Row + 1; row <= _endCell.Row; row++)
                {
                    var vehicle = (T)Activator.CreateInstance(typeof(T));
                    foreach (var header in headers)
                    {
                        vehicle.GetType().InvokeMember(nameof(vehicle.StateNumber),
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                        Type.DefaultBinder, vehicle, new object[] { "ss"});
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return cars;
        }


        /// <summary>
        /// According to the type(T), this fuction get special headers for related object.
        /// If type(T) is not provided for it returns null by default.
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IDictionary of desired cells (where key="name of header", value="address of cell")
        /// else it returns null</returns>
        private IDictionary<string, ExcelCellAddress> GetHeadersAddress<T>() where T : class
        {
            switch (typeof(T))
            {
                case Type carType when carType == typeof(Car):
                    HeaderNames.GetPropValue("Номер единицы оборудования");
                    return GetHeadersAddress(HeaderNames.UnitNumber, HeaderNames.UnitModel, HeaderNames.StateNumber);

                default:
                    return null;
            }
        }


        /// <summary>
        /// This function gets headers` names and find them in excel headers (first row).
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IDictionary where key="name of header", value="address of cell"</returns>
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
                        var addr = (from cell in _worksheet.Cells[_startCell.Row, _startCell.Column, _startCell.Row, _endCell.Column]
                                    where cell.Text == header
                                    select cell.Start).First();
                        headersCells.Add(header, addr);
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
