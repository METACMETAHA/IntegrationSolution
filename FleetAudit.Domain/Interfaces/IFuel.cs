using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetAudit.Domain.Interfaces
{
    public interface IFuel
    {
        string Title { get; }

        double DepartureBalance { get; set; }
        double ReturnBalance { get; set; }
        double ConsumptionActual { get; set; }
        double ConsumptionNormative { get; set; }
        double ConsumptionSavingsOrOverruns { get; set; }
    }
}
