using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{
    public interface IPerson
    {
        string UnitNumber { get; set; }

        string FirstName { get; set; }

        string LastName { get; set; }

        string Patronymic { get; set; }
    }
}
