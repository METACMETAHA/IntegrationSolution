using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations.Fuel
{
    public class LPG : FuelBase<Gas>
    {
        public LPG(string FuelName) : base(FuelName)
        { }
    }
}
