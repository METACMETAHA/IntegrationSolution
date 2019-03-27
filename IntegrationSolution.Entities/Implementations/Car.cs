﻿using IntegrationSolution.Common.Converters;
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
        public string StructureName { get; set; }

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

            Trip trip = new Trip();

            foreach (var item in Trips)
            {
                trip.TotalMileage += item.TotalMileage;
                trip.MotoHoursIndicationsAtAll += 
                    (item.MotoHoursIndicationsAtAll == 0 && 
                    item.DepartureMotoHoursIndications != item.ReturnMotoHoursIndications &&
                    item.DepartureMotoHoursIndications != 0) ?
                    item.ReturnMotoHoursIndications - item.DepartureMotoHoursIndications : item.MotoHoursIndicationsAtAll;

                foreach (var fuel in item.FuelDictionary)
                {
                    if (!trip.FuelDictionary.ContainsKey(fuel.Key))
                        trip.FuelDictionary.Add(fuel.Key, new FuelBase(fuel.Key.ToString()));

                    trip.FuelDictionary[fuel.Key].ConsumptionActual += fuel.Value.ConsumptionActual;
                }

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


        public IList<Trip> TripsWithMotoHoursDeviation()
        {
            if (Trips == null || Trips.Count == 0)
                return null;

            IList<Trip> result = new List<Trip>();
            foreach (var item in Trips)
            {
                var deviation = item.ReturnMotoHoursIndications - item.DepartureMotoHoursIndications;
                if (deviation != item.MotoHoursIndicationsAtAll)
                    result.Add(item);
            }
            return result;
        }
    }
}