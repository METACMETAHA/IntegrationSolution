using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace WialonBase.Entities
{
    public class SpeedViolationWialon : TripBaseWialon
    {
        public TimeSpan Duration { get; set; }

        public int SpeedLimit { get; set; }
        
        public SpeedViolationWialon()
        { }
    }
}
