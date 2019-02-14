using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Interfaces;
using System.Linq;

namespace IntegrationSolution.Entities.Implementations
{
    public abstract class FuelBase<T> : IFuel where T : class
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
        public FuelBase(string FuelName = "")
        {
            if (!string.IsNullOrWhiteSpace(FuelName))
                Title = FuelName;
            else
                Title = typeof(T).Name;

            foreach (var prop in this.GetType().GetProperties().Where(x => x.GetCustomAttributes(typeof(HeaderAttribute), false).Length > 0))
                AttributeProvider.SetHeaderDescription<T>(prop.Name, typeof(T).Name);
        }
    }
}
