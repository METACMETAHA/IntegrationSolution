using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{
    public interface IFuel
    {
        string Title { get; }

        string DepartureBalance { get; set; }
        string ReturnBalance { get; set; }
        string ConsumptionActual { get; set; }
        string ConsumptionNormative { get; set; }
        string ConsumptionSavingsOrOverruns { get; set; }
    }
}
