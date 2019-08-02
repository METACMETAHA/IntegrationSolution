using IntegrationSolution.Common.Implementations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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
        
        private string pathToMainFile;
        public string PathToMainFile
        {
            get { return pathToMainFile; }
            set
            {
                pathToMainFile = value;
                OnPropertyChanged();
            }
        }
        
        public SerializeConfigDTO() : base("sys.dat")
        {
            var obj = Deserialize();

            if (obj != null)
            {
                foreach (var prop in obj.GetType().GetProperties())
                {
                    try
                    {
                        this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(obj));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Exception while Deserialize() property: {ex.Message}");
                    }
                }
            }
            else
            {
                HeaderNamesChanged = new Dictionary<string, string>();
            }
        }

        public void Dispose()
        {
            if (this.HeaderNamesChanged != null && !HeaderNamesChanged.Any())
                HeaderNamesChanged = null;
            
            Serialize(this);
        }
    }
}
