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
    public class IntegratedVehicleInfo : VehicleInfoBase, IComparable
    {
        public double? WialonMileageTotal { get; set; }
        public int CountSpeedViolations { get; set; }
               
        public double? PercentDifference
        {
            get
            {
                if (IndicatorMileage == null || IndicatorMileage.SAP == null || IndicatorMileage.Wialon == null
                    || IndicatorMileage.SAP == 0 || IndicatorMileage.Wialon == 0)
                    return null;
                
                return IndicatorMileage.SAP.Value.GetPercentFrom(IndicatorMileage.Wialon.Value);
            }
        }

        public IntegratedVehicleInfo(ICommonCompareIndicator<double?> indicatorMileage,
            ICommonCompareIndicator<int> indicatorCountTrips) : base(indicatorMileage, indicatorCountTrips)
        { }

        public int CompareTo(object obj)
        {
            var vehicle = obj as IntegratedVehicleInfo;
            if (vehicle == null)
                throw new Exception("Невозможно сравнить два объекта");

            if (vehicle.PercentDifference == null)
                return -1;
            else if (this.PercentDifference == null)
                return 1;

            return this.PercentDifference.Value.CompareTo(vehicle.PercentDifference.Value);
        }
    }
}
