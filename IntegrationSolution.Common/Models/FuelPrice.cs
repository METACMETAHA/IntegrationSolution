using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Models
{
    public class FuelPrice : INotifyPropertyChanged
    {
        private double _gasCost;
        public double GasCost
        {
            get { return _gasCost; }
            set
            {
                _gasCost = value;
                OnPropertyChanged();

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
