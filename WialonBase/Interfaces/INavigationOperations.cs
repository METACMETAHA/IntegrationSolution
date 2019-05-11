using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WialonBase.Entities;

namespace WialonBase.Interfaces
{
    public interface INavigationOperations : IConnection
    {
        ICollection<CarWialon> GetCarsEnumarable();

        TripWialon GetCarInfo(int ID, DateTime from, DateTime to);


        /// <summary>
        /// There can be only one report result in a session. 
        /// That is why if a session contains results of previous execution, those results must be cleared before a new execution.
        /// </summary>
        /// <returns>True or False</returns>
        bool CleanUpResults();
    }
}
