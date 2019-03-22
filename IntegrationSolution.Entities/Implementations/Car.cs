using IntegrationSolution.Common.Converters;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IntegrationSolution.Entities.Implementations
{
    public class Car : IVehicle
    {
        public string UnitModel { get; set; }
        public string UnitNumber { get; set; }
        public string Type { get; set; }
        public string Department { get; set; }

        private string _stateNumber;
        public string StateNumber { get => _stateNumber; set => _stateNumber = StateNumberConverter.ToStateNumber(value); }

        public Trip TripResulted { get => CalcTripResulted(); }
        public ICollection<Trip> Trips { get; set; }

        public Car()
        {
            Trips = new Collection<Trip>();
        }


        private Trip CalcTripResulted()
        {
            if (Trips == null || Trips.Count == 0)
                return null;

            Trip trip = new Trip();
            foreach (var item in Trips)
            {

            }

            return trip;
        }
    }
}
