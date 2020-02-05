using IntegrationSolution.Entities.Interfaces.Wialon;
using System;

namespace IntegrationSolution.Entities.Implementations.Wialon
{
    public class TripBaseWialon : ITripWialon
    {
        public DateTime Begin { get; set; }
        
        public string LocationBegin { get; set; }
        public double Mileage { get; set; }
        public int MaxSpeed { get; set; }

        public TripBaseWialon()
        { }
    }
}
