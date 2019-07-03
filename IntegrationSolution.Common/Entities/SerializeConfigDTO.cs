using IntegrationSolution.Common.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Attributes;

namespace IntegrationSolution.Common.Entities
{
    [Serializable]
    public class SerializeConfigDTO : SerializablePropertyChanged<SerializeConfigDTO>, IDisposable
    {
        private Dictionary<string, string> headerNamesChanged;
        public Dictionary<string, string> HeaderNamesChanged
        {
            get { return headerNamesChanged; }
            set
            {
                headerNamesChanged = value;
                OnPropertyChanged();
            }
        }

        public SerializeConfigDTO() : base("sys.dat")
        {
            var obj = Deserialize();

            if (obj != null)
            {
                InitializeHeaders(obj.HeaderNamesChanged);
            }
            else
            {
                HeaderNamesChanged = new Dictionary<string, string>();
            }
        }

        private void InitializeHeaders(Dictionary<string, string> headers)
        {
            if (!headers.Any())
                return;

            HeaderNamesChanged = headers;
        }

        public void Dispose()
        {
            Serialize(this);
        }
    }
}
