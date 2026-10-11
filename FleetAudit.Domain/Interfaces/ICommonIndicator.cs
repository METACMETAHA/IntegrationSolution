using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetAudit.Domain.Interfaces
{
    public interface ICommonIndicator
    {
        double TotalMileage { get; set; }
    }
}
