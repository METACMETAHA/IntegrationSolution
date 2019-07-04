using IntegrationSolution.Common.Implementations;
using System;
using System.Collections.Generic;
using System.IO;
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
            get { return headerNamesChanged ?? new Dictionary<string, string>(); }
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
                if (!obj.HeaderNamesChanged.Any())
                    File.Delete("sys.dat");
                else
                    InitializeHeaders(obj.HeaderNamesChanged);
            }
            else
            {
                HeaderNamesChanged = new Dictionary<string, string>();
            }
        }

        private void InitializeHeaders(Dictionary<string, string> headers)
        {
            if (headers == null || !headers.Any())
                return;

            HeaderNamesChanged = headers;
        }

        public void Dispose()
        {
            Serialize(this);
        }
    }
}
