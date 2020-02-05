using IntegrationSolution.Entities.SelfEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations
{
    public class CompareTripSAP : IEqualityComparer<TripSAP>
    {
        public bool Equals(TripSAP x, TripSAP y)
        {
            return (x.Driver == y.Driver) && (x.TotalMileage == y.TotalMileage)
                && (x.DepartureFromGarageDate == y.DepartureFromGarageDate)
                && (x.DepartureOdometerValue == y.DepartureOdometerValue)
                && (x.ReturnOdometerValue == y.ReturnOdometerValue)
                && (x.MotoHoursIndicationsAtAll == y.MotoHoursIndicationsAtAll)
                && (x.ReturnToGarageDate == y.ReturnToGarageDate);
        }

        public int GetHashCode(TripSAP obj)
        {
            return obj.TotalMileage.GetHashCode() + obj.Driver.GetHashCode() + obj.DepartureFromGarageDate.GetHashCode()
                + obj.ReturnToGarageDate.GetHashCode() + obj.MotoHoursIndicationsAtAll.GetHashCode();
        }
    }
}
