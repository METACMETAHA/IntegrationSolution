using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WialonBase.Entities;

namespace WialonBase.Interfaces
{
    public interface INavigationOperations
    {
        IEnumerable<CarWialon> GetCarsEnumarable();
    }
}
