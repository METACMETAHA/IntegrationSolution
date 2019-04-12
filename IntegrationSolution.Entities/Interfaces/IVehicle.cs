using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.SelfEntities;
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
        Trip TripResulted { get; }

        ICollection<Trip> Trips { get; set; }

        string UnitNumber { get; set; }

        string UnitModel { get; set; }

        string StateNumber { get; set; }

        string Type { get; set; }

        string Department { get; set; }

        string StructureName { get; set; }
    }
}
