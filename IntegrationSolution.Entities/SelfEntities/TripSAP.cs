using IntegrationSolution.Common.Enums;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.SelfEntities
{
    /// <summary>
    /// Description of current properties is in HeaderNames file
    /// </summary>
    public class TripSAP : 
        IMoveDateTimeCheck, 
        ICarIndicator, 
        ICommonIndicator, 
        ITractorIndicators,
        IComparable
    {
        public double DepartureOdometerValue { get; set; }
        public double ReturnOdometerValue { get; set; }
        public double TotalMileage { get; set; }

        public double DepartureMotoHoursIndications { get; set; }
        public double ReturnMotoHoursIndications { get; set; }
        public double MotoHoursIndicationsAtAll { get; set; }

        public DateTime DepartureFromGarageDate { get; set; }
        public DateTime ReturnToGarageDate { get; set; }
        public TimeSpan TimeOnDutyAtAll { get => ReturnToGarageDate - DepartureFromGarageDate; }

        ICommonCompareIndicator<double> IndicatorMileage { get; set; }

        public Driver Driver { get; set; }

        public Dictionary<FuelEnum, IFuel> FuelDictionary { get; set; }
        
        public TripSAP(ICommonCompareIndicator<double> indicatorMileage)
        {
            FuelDictionary = new Dictionary<FuelEnum, IFuel>();
            IndicatorMileage = indicatorMileage;
            Driver = new Driver();
        }

        public int CompareTo(object obj)
        {
            var trip = obj as TripSAP;
            if(trip == null)
                throw new Exception("Невозможно сравнить два объекта");

            return this.TotalMileage.CompareTo(trip.TotalMileage);
        }
    }
}
