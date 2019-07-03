using IntegrationSolution.Common.Entities;
using IntegrationSolution.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Implementations
{
    public class AppConfiguration : PropertyChangedBase, ISettings
    {
        public SerializeConfigDTO ConfigDTO { get; set; }

        public AppConfiguration(SerializeConfigDTO configDTO)
        {
            ConfigDTO = configDTO;
        }

        public object this[string propertyName]
        {
            get
            {
                return ConfigurationManager.AppSettings[propertyName];
            }
            set
            {
                ConfigurationManager.AppSettings.Set(propertyName, value.ToString());
            }
        }
        
    }
}
