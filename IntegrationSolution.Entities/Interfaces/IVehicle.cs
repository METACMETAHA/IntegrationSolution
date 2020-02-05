using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{
    public interface IVehicle
    {
        string UnitModel { get; set; }

        string StateNumber { get; set; }

        string Type { get; set; }

        string StructureName { get; set; }
    }
}
