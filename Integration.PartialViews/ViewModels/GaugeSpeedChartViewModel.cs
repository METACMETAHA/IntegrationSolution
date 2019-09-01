using IntegrationSolution.Common.ModulesExtension.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.PartialViews.ViewModels
{
    public class GaugeSpeedChartViewModel : ChartsVMBase
    {
        private double currentValue;
        public double CurrentValue
        {
            get { return currentValue; }
            set { SetProperty(ref currentValue, value); }
        }

        private double fromValue;
        public double FromValue
        {
            get { return fromValue; }
            set { SetProperty(ref fromValue, value); }
        }

        private double limitValue;
        public double LimitValue
        {
            get { return limitValue; }
            set { SetProperty(ref limitValue, value); }
        }

        private double toValue;
        public double ToValue
        {
            get { return toValue; }
            set { SetProperty(ref toValue, value); }
        }

        public GaugeSpeedChartViewModel() : this(0)
        { }

        public GaugeSpeedChartViewModel(double current, double from = 0, double to = 200, double limit = 80)
        {
            if (current > to)
                CurrentValue = to;
            else
                CurrentValue = current;
            FromValue = from;
            ToValue = to;
            LimitValue = limit;
        }
    }
}
