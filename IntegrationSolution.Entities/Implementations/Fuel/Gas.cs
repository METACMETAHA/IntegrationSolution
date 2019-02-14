using IntegrationSolution.Entities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations.Fuel
{
    public class Gas : FuelBase<Gas>
    {
        public Gas(string FuelName) : base(FuelName)
        { }        
    }
}
