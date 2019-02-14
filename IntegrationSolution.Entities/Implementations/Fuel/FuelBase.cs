using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Implementations.Fuel;
using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations
{
    public class FuelBase<T> : IFuel where T : class
    {
        public string Title { get; private set; }

        [HeaderAttribute("DepartureBalance&replace&")]
        public string DepartureBalance { get; set; }

        [HeaderAttribute("ReturnBalance&replace&")]
        public string ReturnBalance { get; set; }

        [HeaderAttribute("Consumption&replace&Actual")]
        public string ConsumptionActual { get; set; }

        [HeaderAttribute("Consumption&replace&Normative")]
        public string ConsumptionNormative { get; set; }

        [HeaderAttribute("Consumption&replace&SavingsOrOverruns")]
        public string ConsumptionSavingsOrOverruns { get; set; }


        /// <summary>
        /// In this constructor it creates child of FuelBase and sets
        /// them related attribute`s value of ef each property which has [HeaderAttribute]
        /// </summary>
        public FuelBase(string FuelName)
        {
            Title = FuelName;

            foreach (var prop in this.GetType().GetProperties().Where(x => x.GetCustomAttributes(typeof(HeaderAttribute), false).Length > 0))
                AttributeProvider.SetHeaderDescription<T>(prop.Name, typeof(T).Name);
        }
    }
}
