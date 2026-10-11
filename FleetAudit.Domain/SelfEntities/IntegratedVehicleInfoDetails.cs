using FleetAudit.Domain.Implementations;
using FleetAudit.Domain.Implementations.Wialon;
using FleetAudit.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetAudit.Domain.SelfEntities
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
