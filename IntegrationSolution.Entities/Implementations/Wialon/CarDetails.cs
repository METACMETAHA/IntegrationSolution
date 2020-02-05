using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations.Wialon
{
    public class CarDetails
    {
        public int ID { get; private set; }
        public int MaxSpeed { get; set; }
        public int Mileage { get; set; }


        public CarDetails(int ID)
        {
            this.ID = ID;
        }
    }
}
