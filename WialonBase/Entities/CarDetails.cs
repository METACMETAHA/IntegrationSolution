using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WialonBase.Entities
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
