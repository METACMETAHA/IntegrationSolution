using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Excel.Common;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Implementations
{
    public class ExcelCarOperations : ExcelBase, ICarOperations
    {
        public ExcelCarOperations(ExcelPackage excelPackage) : base(excelPackage)
        { }


        /// <summary>
        /// This function gets all IVehicle-objects from excel file which is storage of cars.
        /// Fill in object: Тип, Гос.знак единицы оборудования, Марка
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IEnumerable of IVehicle</returns>
        public IEnumerable<IVehicle> GetVehicles()
        {
            ICollection<IVehicle> cars = new List<IVehicle>();
            try
            {
                IDictionary<string, ExcelCellAddress> headers = CommonOperationsBase.GetHeadersAddress(
                    this, 
                    HeaderNames.TypeOfVehicle, 
                    HeaderNames.ModelOfVehicle, 
                    HeaderNames.StateNumber,
                    HeaderNames.Departments);

                for (int row = headers.First().Value.Row + 1; row < this.EndCell.Row; row++)
                {
                    IVehicle vehicle = new Car();
                    foreach (var item in headers)
                    {
                        try
                        {
                            switch (item.Key)
                            {
                                case nameof(HeaderNames.TypeOfVehicle):
                                    vehicle.Type = this.WorkSheet.Cells[row, item.Value.Column].Text;
                                    break;

                                case nameof(HeaderNames.ModelOfVehicle):
                                    vehicle.UnitModel = this.WorkSheet.Cells[row, item.Value.Column].Text;
                                    break;

                                case nameof(HeaderNames.StateNumber):
                                    vehicle.StateNumber = this.WorkSheet.Cells[row, item.Value.Column].Text;
                                    break;

                                case nameof(HeaderNames.Departments):
                                    vehicle.Department = this.WorkSheet.Cells[row, item.Value.Column].Text;
                                    break;

                                default:
                                    continue;
                            }
                        }
                        catch (Exception)
                        { }
                    }
                    if (!string.IsNullOrWhiteSpace(vehicle.StateNumber))
                        cars.Add(vehicle);
                }


                //for (int row = _startCell.Row + 1; row <= _endCell.Row; row++)
                //{
                //    var vehicle = (T)Activator.CreateInstance(typeof(T));
                //    foreach (var header in headers)
                //    {
                //        vehicle.GetType().InvokeMember(header.Key,
                //        BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                //        Type.DefaultBinder, vehicle, new object[] { _worksheet.Cells[row, header.Value.Column].Text });
                //    }

                //    vehicle.Fuels = GetFuelByVehicle(row);

                //    cars.Add(vehicle);
                //}
            }
            catch (Exception)
            {

                throw;
            }
            return cars;
        }
        
    }
}
