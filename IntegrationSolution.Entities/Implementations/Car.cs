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

        private string _stateNumber;
        public string StateNumber { get => _stateNumber; set => _stateNumber = Converter.ToStateNumber(value); }
    }
}
