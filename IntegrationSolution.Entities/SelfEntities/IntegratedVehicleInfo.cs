using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.SelfEntities
{
    public class IntegratedVehicleInfo : VehicleInfoBase
    {
        public double? WialonMileageTotal { get; set; }
        public int CountSpeedViolations { get; set; }
               
        public double? PercentDifference
        {
            get
            {
                if (IndicatorMileage == null || IndicatorMileage.SAP == null || IndicatorMileage.Wialon == null)
                    return null;
                
                return IndicatorMileage.SAP.Value.GetPercentFrom(IndicatorMileage.Wialon.Value);
            }
        }

        public IntegratedVehicleInfo(ICommonCompareIndicator<double?> indicatorMileage,
            ICommonCompareIndicator<int> indicatorCountTrips) : base(indicatorMileage, indicatorCountTrips)
        { }
    }
}
