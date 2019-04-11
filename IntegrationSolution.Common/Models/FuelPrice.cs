using IntegrationSolution.Common.Implementations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Models
{
    public class FuelPrice : PropertyChangedBase
    {
        private double _gasCost;
        public double GasCost
        {
            get { return _gasCost; }
            set
            {
                _gasCost = value;
                NotifyOfPropertyChange();
            }
        }

        private double _diselCost;
        public double DiselCost
        {
            get { return _diselCost; }
            set
            {
                _diselCost = value;
                NotifyOfPropertyChange();
            }
        }

        private double _lpgCost;
        public double LPGCost
        {
            get { return _lpgCost; }
            set
            {
                _lpgCost = value;
                NotifyOfPropertyChange();
            }
        }


        public FuelPrice()
        {
            //GasCost = DiselCost = LPGCost = 1;
        }
    }
}
