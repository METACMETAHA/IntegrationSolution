using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{
    public interface ITractorIndicators
    {
        string DepartureMotoHoursIndications { get; set; }
        string ReturnMotoHoursIndications { get; set; }
        string MotoHoursIndicationsAtAll { get; set; }
    }
}
