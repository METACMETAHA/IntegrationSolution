using IntegrationSolution.Entities.Implementations.Wialon;
using System;
using System.Collections.Generic;

namespace WialonBase.Interfaces
{
    public interface INavigationOperations : IConnection
    {
        ICollection<CarWialon> GetCarsEnumarable();

        /// <summary>
        /// Get total(main) information about trip by car ID
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        TripWialon GetCarInfo(int ID, DateTime from, DateTime to);

        /// <summary>
        /// Get total(main) information about trip by car ID
        /// and details about each trip in period from to.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        TripWialon GetCarInfoDetails(int ID, DateTime from, DateTime to);
        
        /// <summary>
        /// There can be only one report result in a session. 
        /// That is why if a session contains results of previous execution, those results must be cleared before a new execution.
        /// </summary>
        /// <returns>True or False</returns>
        bool CleanUpResults();
    }
}
