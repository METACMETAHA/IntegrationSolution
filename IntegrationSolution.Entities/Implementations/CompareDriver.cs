using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations
{
    public class CompareDriver : IEqualityComparer<Driver>
    {
        public bool Equals(Driver x, Driver y)
        {
            return (x.UnitNumber == y.UnitNumber) && (x.LastName == y.LastName);
        }

        public int GetHashCode(Driver obj)
        {
            return obj.LastName.GetHashCode() + obj.UnitNumber.GetHashCode();
        }
    }
}
