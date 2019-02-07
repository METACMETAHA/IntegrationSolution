using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{
    public interface IVehicle
    {
        string UnitNumber { get; set; }

        string ModelVehicle { get; set; }

        string StateNumber { get; set; }
    }
}
