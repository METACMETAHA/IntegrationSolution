using FleetAudit.Domain.Interfaces.Wialon;
using System;

namespace FleetAudit.Domain.Implementations.Wialon
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
