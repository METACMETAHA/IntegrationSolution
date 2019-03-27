using IntegrationSolution.Common.Enums;
using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Interfaces
{
    public interface ICarOperations : IDisposable
    {
        /// <summary>
        /// This function gets all IVehicle-objects from excel file which is storage of cars.
        /// Fill next properties of object: Тип, Гос.знак единицы оборудования, Марка
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>IEnumerable of IVehicle</returns>
        IEnumerable<IVehicle> GetVehicles();


        /// <summary>
        /// Fill by Trips and Fuel
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        bool FillVehicleAvaliableData(ref IVehicle vehicle);


        void FillFullDataColumns(ICollection<IVehicle> vehicles);


        void FillTotalResults(ICollection<IVehicle> vehicles);
    }
}
