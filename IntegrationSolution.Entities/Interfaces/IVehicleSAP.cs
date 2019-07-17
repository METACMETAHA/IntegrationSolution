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
    public interface IVehicleSAP : IVehicle
    {
        TripSAP TripResulted { get; }

        ICollection<TripSAP> Trips { get; set; }

        int? CountTrips { get; }

        string UnitNumber { get; set; }
        
        string Department { get; set; }
    }
}
