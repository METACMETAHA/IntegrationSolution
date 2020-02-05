using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations.Wialon
{
    public class CarWialon
    {
        public int ID { get; private set; }
        public string StateNumber { get; private set; }


        public CarWialon(int ID, string StateNumber)
        {
            this.ID = ID;
            this.StateNumber = StateNumber;
        }
    }
}
