using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Interfaces
{
    public interface IExcelWriter
    {
        // Report about difference between SAP and Wialon 
        void CreateReportDiffMileage(string path, List<IntegratedVehicleInfo> valuePairs, double BadPercent = 5,
            List<IVehicleSAP> sapCars = null, List<CarWialon> wialonCars = null);

        // Report WITH details about difference between SAP and Wialon
        void CreateReportDiffMileageWithDetails(string path, List<IntegratedVehicleInfoDetails> valuePairs, double BadPercent = 5,
            List<IVehicleSAP> sapCars = null, List<CarWialon> wialonCars = null);
    }
}
