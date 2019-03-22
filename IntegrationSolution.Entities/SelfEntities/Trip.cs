using IntegrationSolution.Common.Enums;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.SelfEntities
{
    /// <summary>
    /// Description of current properties is in HeaderNames file
    /// </summary>
    public class Trip : ICarIndicator, ICommonIndicator, IMoveDateTimeCheck, ITractorIndicators
    {
        public string DepartureOdometerValue { get; set; }
        public string ReturnOdometerValue { get; set; }
        public string TotalMileage { get; set; }
        public string DepartureMotoHoursIndications { get; set; }
        public string ReturnMotoHoursIndications { get; set; }
        public string MotoHoursIndicationsAtAll { get; set; }

        public string DepartureFromGarageDate { get; set; }
        public string DepartureFromGarageTime { get; set; }
        public string ReturnToGarageDate { get; set; }
        public string ReturnToGarageTime { get; set; }
        public string TimeOnDutyAtAll { get; set; }

        public Dictionary<FuelEnum, IFuel> FuelDictionary { get; set; }
        public Driver Driver { get; set; }

        public Trip()
        {
            Driver = new Driver();
            FuelDictionary = new Dictionary<FuelEnum, IFuel>();
        }
    }
}
