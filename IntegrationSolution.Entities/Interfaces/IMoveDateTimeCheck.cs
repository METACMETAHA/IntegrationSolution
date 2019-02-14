using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{
    public interface IMoveDateTimeCheck
    {
        string DepartureFromGarageDate { get; set; }
        string DepartureFromGarageTime { get; set; }
        string ReturnToGarageDate { get; set; }
        string ReturnToGarageTime { get; set; }
        string TimeOnDutyAtAll { get; set; }
    }
}
