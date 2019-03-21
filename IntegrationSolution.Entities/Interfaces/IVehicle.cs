using IntegrationSolution.Entities.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{   
    public interface IVehicle
    {
        Dictionary<string, IFuel> Fuels { get; set; }

        string UnitNumber { get; set; }

        string UnitModel { get; set; }

        string StateNumber { get; set; }

        string Type { get; set; }

        string Department { get; set; }
    }
}
