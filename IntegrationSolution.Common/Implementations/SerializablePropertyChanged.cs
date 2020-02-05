using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Implementations
{
    [Serializable]
    public class SerializablePropertyChanged<T> : Serializable<T>, INotifyPropertyChanged where T : class
    {
        public SerializablePropertyChanged(string binaryFile) : base(binaryFile)
        { }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
