using System;
using System.Collections.Generic;

namespace IntegrationSolution.Entities.Implementations.Wialon
{
    public class TripWialon : TripBaseWialon
    {
        public DateTime Finish { get; set; }
        public string LocationFinish { get; set; }
        
        public double TotalMileage { get; set; }
        public int AvgSpeed { get; set; }
        public int TotalMaxSpeed { get; set; }
        
        public int CountTrips { get; set; }
        public IEnumerable<TripWialon> Trips { get; set; }
        public IEnumerable<SpeedViolationWialon> SpeedViolation { get; set; }


        public TripWialon()
        {
            //SpeedViolation = new SpeedViolationWialon();
        }
    }
}
