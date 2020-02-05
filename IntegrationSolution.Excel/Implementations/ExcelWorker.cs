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
    public class ExcelWorker : ExcelBase
    {
        //public ExcelWorker(ExcelPackage excelPackage)
        //{
        //    Excel = excelPackage;
        //}


        public void Dispose()
        {
            base.Dispose();
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


        //public double Get
        

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


        

        
        

        private void GetStateNumber()
        {

        }


    }
}
