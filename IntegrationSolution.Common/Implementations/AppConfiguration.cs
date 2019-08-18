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

        private Configuration configurationFile;
        
        public AppConfiguration(SerializeConfigDTO configDTO)
        {
            ConfigDTO = configDTO;

            configurationFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public object this[string propertyName]
        {
            get
            {
                return ConfigurationManager.AppSettings[propertyName];
            }
            set
            {
                configurationFile.AppSettings.Settings[propertyName].Value = value.ToString();
                configurationFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
        
    }
}
