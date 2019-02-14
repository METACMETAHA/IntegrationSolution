using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations.Fuel
{
    public class Disel : FuelBase<Disel>
    {
        public Disel(string FuelName) : base(FuelName)
        { } 
    }
}
