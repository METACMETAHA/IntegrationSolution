using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations
{
    public class CompareVehicles : IEqualityComparer<IVehicle>
    {
        public bool Equals(IVehicle x, IVehicle y)
        {
            return x.StateNumber == y.StateNumber;
        }

        public int GetHashCode(IVehicle obj)
        {
            return obj.StateNumber.GetHashCode() + obj.Type.GetHashCode() + obj.UnitModel.GetHashCode();
        }
    }
}
