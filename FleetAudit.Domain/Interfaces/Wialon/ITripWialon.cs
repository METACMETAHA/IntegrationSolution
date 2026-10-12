using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetAudit.Domain.Interfaces.Wialon
{
    public interface ITripWialon
    {
        DateTime Begin { get; set; }

        string LocationBegin { get; set; }
        double Mileage { get; set; }
        int MaxSpeed { get; set; }
    }
}
