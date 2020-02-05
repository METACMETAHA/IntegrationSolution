using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{
    public interface ITractorIndicators
    {
        double DepartureMotoHoursIndications { get; set; }
        double ReturnMotoHoursIndications { get; set; }
        double MotoHoursIndicationsAtAll { get; set; }
    }
}
