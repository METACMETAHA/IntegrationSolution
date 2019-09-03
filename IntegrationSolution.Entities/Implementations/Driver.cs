using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Entities.SelfEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations
{
    public class Driver : PropertyChangedBase, IPerson, IComparable
    {
        public string UnitNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }

        private ConcurrentObservableDictionary<IVehicle, List<TripSAP>> historyDrive;
        public ConcurrentObservableDictionary<IVehicle, List<TripSAP>> HistoryDrive
        {
            get { return historyDrive; }
            set
            {
                historyDrive = value;
                NotifyOfPropertyChange();
            }
        }

        public override string ToString()
        {
            return $"{LastName} {FirstName} {Patronymic}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType()) return false;

            Driver dr = (Driver)obj;
            return (this.UnitNumber == dr.UnitNumber) && (this.LastName == dr.LastName);
        }

        public int CompareTo(object obj)
        {
            Driver driver = obj as Driver;
            if(driver == null)
                throw new Exception("Невозможно сравнить два объекта");

            var totalCurrent = this.HistoryDrive?.Select(x => x.Value.Sum(zx => zx.TotalMileage)).First();
            var totalObj = driver.HistoryDrive?.Select(x => x.Value.Sum(zx => zx.TotalMileage)).First();

            return totalCurrent.Value.CompareTo(totalObj.Value);
        }

        public double AvarageMileagePerTrip
        {
            get
            {
                if (HistoryDrive == null)
                    return 0;

                List<double> avg = new List<double>();
                foreach (var item in HistoryDrive)
                    avg.Add(item.Value.Average(x => x.TotalMileage));

                return avg.Average();
            }
        }

        public KeyValuePair<double, DateTime> MaxTripMileage
        {
            get
            {
                if (HistoryDrive == null)
                    return new KeyValuePair<double, DateTime>(0, DateTime.MinValue);

                List<TripSAP> max = new List<TripSAP>();
                foreach (var item in HistoryDrive)
                    max.AddRange(item.Value.Where(x => Math.Abs(x.TotalMileage - item.Value.Max(z => z.TotalMileage)) < double.Epsilon));

                var max_obj = max.Where(x => Math.Abs(x.TotalMileage - max.Max(y => y.TotalMileage)) < double.Epsilon).FirstOrDefault();
                return new KeyValuePair<double, DateTime>(max_obj.TotalMileage, max_obj.DepartureFromGarageDate);
            }
        }

        public KeyValuePair<double, DateTime> MinTripMileage
        {
            get
            {
                if (HistoryDrive == null)
                    return new KeyValuePair<double, DateTime>(0, DateTime.MinValue);

                List<TripSAP> min = new List<TripSAP>();
                foreach (var item in HistoryDrive)
                    min.AddRange(item.Value.Where(x => Math.Abs(x.TotalMileage - item.Value.Min(z => z.TotalMileage)) < double.Epsilon));

                var min_obj = min.Where(x => Math.Abs(x.TotalMileage - min.Min(y => y.TotalMileage)) < double.Epsilon).FirstOrDefault();
                return new KeyValuePair<double, DateTime>(min_obj.TotalMileage, min_obj.DepartureFromGarageDate);
            }
        }

        public int CountTrips
            => HistoryDrive?.Values.FirstOrDefault().Count ?? 0;

        public int CountCars
            => HistoryDrive?.Keys.Count ?? 0;
    }
}
