using IntegrationSolution.Common.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WialonBase.Entities;

namespace WialonBase.Helpers
{
    public static class JsonToWialonEntitiesConverter
    {
        public static CarWialon ToCarWialon(this JToken obj)
        {
            try
            {
                var id = int.Parse(obj["id"].Value<string>());
                var state = obj["nm"].Value<string>().ToStateNumberWialon();

                return new CarWialon(id, state);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static TripWialon ToTripWialon(this JToken obj)
        {
            TripWialon tripWialon = new TripWialon
            {
                TotalMaxSpeed = obj["reportLayer"]["units"].First["max_speed"].Value<int>(),
                TotalMileage = obj["reportLayer"]["units"].First["mileage"].Value<int>(),
                Begin = ((Int32)obj["reportLayer"]["units"].First["msgs"]["first"]["time"].Value<long>()).ToDateTime(),
                Finish = ((Int32)obj["reportLayer"]["units"].First["msgs"]["last"]["time"].Value<long>()).ToDateTime()
            };
            var tables = obj["reportResult"]["tables"];

            byte[] bytes = Encoding.Default.GetBytes(tables.ToString());
            var encodedString = Encoding.UTF8.GetString(bytes);
            tables = JToken.Parse(encodedString);

            var trips = tables.FirstOrDefault(x => x["label"].Value<string>() == "Поездки");

            tripWialon.LocationBegin = trips.ElementAt(9).First.ElementAt(3).Value<string>();
            tripWialon.LocationFinish = trips.ElementAt(9).First.ElementAt(5).Value<string>();
            tripWialon.Mileage = double.Parse(trips.ElementAt(9).First.ElementAt(7).Value<string>().Replace("km/h", "").Replace("km", "").Replace('.', ',').Trim());
            tripWialon.AvgSpeed = int.Parse(trips.ElementAt(9).First.ElementAt(8).Value<string>().Replace("km/h", "").Replace("km", "").Trim());
            tripWialon.MaxSpeed = int.Parse(trips.ElementAt(9).First.ElementAt(9).Value<string>().Replace("km/h", "").Replace("km", "").Trim());

            var speedTrips = tables.FirstOrDefault(x => x["label"].Value<string>() == "Превышение скорости");
            if (speedTrips != null)
            {
                SpeedViolationWialon speed = new SpeedViolationWialon
                {
                    Begin = speedTrips.ElementAt(9).First.ElementAt(1).Value<DateTime>(),
                    LocationBegin = speedTrips.ElementAt(9).First.ElementAt(2).Value<string>(),
                    Duration = TimeSpan.Parse(speedTrips.ElementAt(9).First.ElementAt(3).Value<string>()),
                    MaxSpeed = int.Parse(speedTrips.ElementAt(9).First.ElementAt(5).Value<string>().Replace("km/h", "").Replace("km", "").Trim()),
                    SpeedLimit = int.Parse(speedTrips.ElementAt(9).First.ElementAt(6).Value<string>().Replace("km/h", "").Replace("km", "").Trim()),
                    Mileage = double.Parse(speedTrips.ElementAt(9).First.ElementAt(7).Value<string>().Replace("km/h", "").Replace("km", "").Replace('.', ',').Trim())
                };
                tripWialon.SpeedViolation = speed;
            }

            return tripWialon;
        }
    }
}
