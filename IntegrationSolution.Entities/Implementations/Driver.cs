using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations
{
    public class Driver : IPerson
    {
        public string UnitNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }

        public override string ToString()
        {
            return $"{LastName} {FirstName} {Patronymic}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType()) return false;

            Driver dr = (Driver)obj;
            return (this.UnitNumber == dr.UnitNumber) && (this.LastName == dr.LastName);
        }
    }
}
