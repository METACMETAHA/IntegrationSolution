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
        IEnumerable<IVehicle> GetVehicles();

        bool FillVehicleAvaliableData(ref IVehicle vehicle);

        void AddOrUpdateFuelColumn(IVehicle vehicle);
    }
}
