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

        public Trip TripResulted { get => GetTripResulted(); }
        public ICollection<Trip> Trips { get; set; }

        public Car()
        {
            Trips = new Collection<Trip>();
        }


        private Trip GetTripResulted()
        {
            if (Trips == null || Trips.Count == 0)
                return null;

            Trip trip = new Trip()
            { TotalMileage = 0 };

            foreach (var item in Trips)
            {
                trip.TotalMileage += item.TotalMileage;
            }

            return trip;
        }


        public IList<Trip> TripsWithMileageDeviation()
        {
            if (Trips == null || Trips.Count == 0)
                return null;

            IList<Trip> result = new List<Trip>();
            foreach (var item in Trips)
            {
                var deviation = item.ReturnOdometerValue - item.DepartureOdometerValue;
                if (deviation != item.TotalMileage)
                    result.Add(item);
            }
            return result;
        }
    }
}
