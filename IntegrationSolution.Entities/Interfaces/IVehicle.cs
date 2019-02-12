using IntegrationSolution.Entities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{   
    public interface IVehicle
    {
        string UnitNumber { get; set; }

        string UnitModel { get; set; }

        string StateNumber { get; set; }
        
    }
}
