using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using WialonBase.Entities.Interfaces;

namespace WialonBase.Entities
{
    public class TripWialon : TripBaseWialon
    {
        public DateTime Finish { get; set; }
        public string LocationFinish { get; set; }
        
        public double TotalMileage { get; set; }
        public int AvgSpeed { get; set; }
        public int TotalMaxSpeed { get; set; }
        public SpeedViolationWialon SpeedViolation { get; set; }

        public TripWialon()
        {
            //SpeedViolation = new SpeedViolationWialon();
        }
    }
}
