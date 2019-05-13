using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.SelfEntities
{
    public class IntegratedVehicleInfo
    {
        public string StateNumber { get; set; }
        public string Model { get; set; }
        public int CountTrips { get; set; }
        public double? SAP_Mileage { get; set; }
        public double? Wialon_Mileage { get; set; }
        public double DifferenceMileage { get => (SAP_Mileage - Wialon_Mileage) ?? 0; }
        public double? PercentDifference
        {
            get
            {
                if (SAP_Mileage == null || Wialon_Mileage == null)
                    return null;
                var midSum = (SAP_Mileage + Wialon_Mileage) / 2;
                var result = (DifferenceMileage / midSum) * 100;
                return Math.Round(Math.Abs(result.Value), 2);
            }
        }
    }
}
