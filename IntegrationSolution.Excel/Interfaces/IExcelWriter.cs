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
        void CreateReportDiffMileage(string path, List<IntegratedVehicleInfo> valuePairs, double BadPercent = 5);
        
        void CreateReportDiffMileageWithDetails(string path, List<IntegratedVehicleInfoDetails> valuePairs, double BadPercent = 5);
    }
}
