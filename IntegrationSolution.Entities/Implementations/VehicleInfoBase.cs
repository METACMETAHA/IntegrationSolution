using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations
{
    public abstract class VehicleInfoBase : IVehicle
    {
        public string StateNumber { get; set; }
        public string UnitModel { get; set; }
        public string Type { get; set; }
        public string StructureName { get; set; }
        
        public ICommonCompareIndicator<double?> IndicatorMileage { get; set; }
        public ICommonCompareIndicator<int> CountTrips { get; set; }

        public IEnumerable<SpeedViolationWialon> SpeedViolation { get; set; }


        public VehicleInfoBase(
            ICommonCompareIndicator<double?> indicatorMileage,
            ICommonCompareIndicator<int> indicatorCountTrips)
        {
            IndicatorMileage = indicatorMileage;
            CountTrips = indicatorCountTrips;
        }

        public override string ToString()
        {
            return this.StateNumber;
        }
    }
}
