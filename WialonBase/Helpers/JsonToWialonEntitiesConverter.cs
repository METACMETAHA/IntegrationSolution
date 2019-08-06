using IntegrationSolution.Common.Converters;
using IntegrationSolution.Entities.Implementations.Wialon;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
                TotalMileage = obj["reportLayer"]["units"].First["mileage"].Value<int>()
            };
            try
            {
                tripWialon.Begin = ((Int32)obj["reportLayer"]["units"].First["msgs"]["first"]["time"].Value<long>()).ToDateTime();
                tripWialon.Finish = ((Int32)obj["reportLayer"]["units"].First["msgs"]["last"]["time"].Value<long>()).ToDateTime();
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger(nameof(JsonToWialonEntitiesConverter)).Debug($"{ex.Message} while processing entity (Begin-Finish)");
            }

            var tables = obj["reportResult"]["tables"];
            
            tables = JToken.Parse(tables.ToString());

            var trips = tables.FirstOrDefault(x => x["label"].Value<string>() == "Поездки");

            if (trips != null)
            {
                tripWialon.CountTrips = trips["rows"].Value<int>();

                try
                {
                    tripWialon.Mileage = double.Parse(trips.ElementAt(9).First.ElementAt(7).Value<string>().Replace("km/h", "").Replace("km", "").Replace('.', ',').Trim());
                    tripWialon.AvgSpeed = int.Parse(trips.ElementAt(9).First.ElementAt(8).Value<string>().Replace("km/h", "").Replace("km", "").Trim());
                    tripWialon.MaxSpeed = int.Parse(trips.ElementAt(9).First.ElementAt(9).Value<string>().Replace("km/h", "").Replace("km", "").Trim());
                }
                catch
                {
                    tripWialon.Mileage = double.Parse(trips.ElementAt(9).First.ElementAt(8).Value<string>().Replace("km/h", "").Replace("km", "").Replace('.', ',').Trim());
                    tripWialon.AvgSpeed = int.Parse(trips.ElementAt(9).First.ElementAt(9).Value<string>().Replace("km/h", "").Replace("km", "").Trim());
                    tripWialon.MaxSpeed = int.Parse(trips.ElementAt(9).First.ElementAt(10).Value<string>().Replace("km/h", "").Replace("km", "").Trim());
                }

                tripWialon.LocationBegin = trips.ElementAt(9).First.ElementAt(3).Value<string>();
                tripWialon.LocationFinish = trips.ElementAt(9).First.ElementAt(5).Value<string>();
            }
            
            return tripWialon;
        }


        //------------------------------------------------------------------------------------------------
        //----------------------------Last version before Wialon`s changes--------------------------------
        //------------------------------------------------------------------------------------------------
        //public static TripWialon ToTripWialon(this JToken obj)
        //{
        //    TripWialon tripWialon = new TripWialon
        //    {
        //        TotalMaxSpeed = obj["reportLayer"]["units"].First["max_speed"].Value<int>(),
        //        TotalMileage = obj["reportLayer"]["units"].First["mileage"].Value<int>()
        //    };
        //    try
        //    {
        //        tripWialon.Begin = ((Int32)obj["reportLayer"]["units"].First["msgs"]["first"]["time"].Value<long>()).ToDateTime();
        //        tripWialon.Finish = ((Int32)obj["reportLayer"]["units"].First["msgs"]["last"]["time"].Value<long>()).ToDateTime();
        //    }
        //    catch (Exception ex)
        //    {
        //        log4net.LogManager.GetLogger(nameof(JsonToWialonEntitiesConverter)).Debug($"{ex.Message} while processing entity (Begin-Finish)");
        //    }

        //    var tables = obj["reportResult"]["tables"];

        //    tables = JToken.Parse(tables.ToString());

        //    var trips = tables.FirstOrDefault(x => x["label"].Value<string>() == "Поездки");

        //    if (trips != null)
        //    {
        //        tripWialon.CountTrips = trips["rows"].Value<int>();
        //        tripWialon.Mileage = double.Parse(trips.ElementAt(9).First.ElementAt(7).Value<string>().Replace("km/h", "").Replace("km", "").Replace('.', ',').Trim());
        //        tripWialon.AvgSpeed = int.Parse(trips.ElementAt(9).First.ElementAt(8).Value<string>().Replace("km/h", "").Replace("km", "").Trim());
        //        tripWialon.MaxSpeed = int.Parse(trips.ElementAt(9).First.ElementAt(9).Value<string>().Replace("km/h", "").Replace("km", "").Trim());
        //        tripWialon.LocationBegin = trips.ElementAt(9).First.ElementAt(3).Value<string>();
        //        tripWialon.LocationFinish = trips.ElementAt(9).First.ElementAt(5).Value<string>();
        //    }

        //    return tripWialon;
        //}


        /// <summary>
        /// If no speed violation - it returns -1
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int SpeedViolationIndex(this JToken obj)
        {
            var list = obj["reportResult"]["tables"].ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i]["label"].Value<string>() == "Превышение скорости")
                    return i;
            }
            return -1;
        }


        public static IEnumerable<SpeedViolationWialon> ToSpeedViolationEnumerable(this JToken obj)
        {
            List<SpeedViolationWialon> speedCollection = new List<SpeedViolationWialon>();

            foreach (var item in obj.ToArray())
            {
                try
                {
                    SpeedViolationWialon speedViolation = new SpeedViolationWialon()
                    {
                        Begin = DateTime.Parse(item.First["c"].ToArray()[1]["t"].Value<string>()),
                        LocationBegin = item.First["c"].ToArray()[2]["t"].Value<string>(),
                        Duration = TimeSpan.Parse(item.First["c"].ToArray()[3].Value<string>()),
                        MaxSpeed = int.Parse(item.First["c"].ToArray()[5]["t"].Value<string>().Replace("km/h", "").Trim()),
                        SpeedLimit = int.Parse(item.First["c"].ToArray()[6].Value<string>().Replace("km/h", "").Trim()),
                        Mileage = double.Parse(item.First["c"].ToArray()[7].Value<string>().Replace("km", "").Replace('.', ',').Trim())
                    };
                    speedCollection.Add(speedViolation);
                }
                catch
                { }                
            }

            return (speedCollection.Any())? speedCollection : null;
        }


        public static IEnumerable<TripWialon> ToTripsCollectionWialon(this JToken obj)
        {
            List<TripWialon> trips = new List<TripWialon>();

            foreach (var item in obj.ToArray())
            {
                try
                {
                    TripWialon trip = new TripWialon
                    {
                        Begin = ((Int32)item.First["t1"].Value<long>()).ToDateTime(),
                        Finish = ((Int32)item.First["t2"].Value<long>()).ToDateTime(),

                        LocationBegin = item.First["c"].ElementAt(3)["t"].Value<string>(),
                        LocationFinish = item.First["c"].ElementAt(5)["t"].Value<string>(),
                    };

                    // Get mileage
                    try
                    {
                        trip.Mileage = double.Parse(item.First["c"].ElementAt(7).Value<string>().Replace("km", "").Replace(".", ",").Trim());
                        trip.AvgSpeed = int.Parse(item.First["c"].ElementAt(8).Value<string>().Replace("km/h", "").Trim());
                        trip.MaxSpeed = int.Parse(item.First["c"].ElementAt(9)["t"].Value<string>().Replace("km/h", "").Trim());
                    }
                    catch
                    {
                        trip.Mileage = double.Parse(item.First["c"].ElementAt(8).Value<string>().Replace("km", "").Replace(".", ",").Trim());
                        trip.AvgSpeed = int.Parse(item.First["c"].ElementAt(9).Value<string>().Replace("km/h", "").Trim());
                        trip.MaxSpeed = int.Parse(item.First["c"].ElementAt(10)["t"].Value<string>().Replace("km/h", "").Trim());
                    }
                    trips.Add(trip);
                }
                catch
                { }
            }
            return (trips.Any())? trips : null;
        }
    }
}
