using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.SelfEntities
{
    public class IntegratedVehicleInfoDetails : IntegratedVehicleInfo
    {
        public IEnumerable<TripSAP> TripsSAP { get; set; }
        public IEnumerable<TripWialon> TripsWialon { get; set; }

        public IntegratedVehicleInfoDetails(ICommonCompareIndicator<double?> indicatorMileage, 
            ICommonCompareIndicator<int> indicatorCountTrips) : base(indicatorMileage, indicatorCountTrips)
        { }
    }
}
