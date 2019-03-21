using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations
{
    public class Car : IVehicle
    {
        public string UnitModel { get; set; }
        public string UnitNumber { get; set; }
        public string Type { get; set; }
        public string Department { get; set; }

        private string _stateNumber;
        public string StateNumber { get => _stateNumber; set => _stateNumber = Converter.ToStateNumber(value); }

        public Dictionary<string, IFuel> Fuels { get; set; }

        public Car()
        { }
    }
}
