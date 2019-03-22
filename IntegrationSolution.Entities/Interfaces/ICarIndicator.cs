using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{
    public interface ICarIndicator
    {
        double DepartureOdometerValue { get; set; }
        double ReturnOdometerValue { get; set; }
    }
}
