using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using WialonBase.Entities.Interfaces;

namespace WialonBase.Entities
{
    public class TripBaseWialon : ITripWialon
    {
        public DateTime Begin { get; set; }
        
        public string LocationBegin { get; set; }
        public double Mileage { get; set; }
        public int MaxSpeed { get; set; }

        public TripBaseWialon()
        { }
    }
}
