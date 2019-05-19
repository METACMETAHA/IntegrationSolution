using System;

namespace IntegrationSolution.Entities.Implementations.Wialon
{
    public class SpeedViolationWialon : TripBaseWialon
    {
        public TimeSpan Duration { get; set; }

        public int SpeedLimit { get; set; }
        
        public SpeedViolationWialon()
        { }
    }
}
