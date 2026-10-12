using FleetAudit.Common.Entities;
using FleetAudit.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FleetAudit.Common.Implementations
{
    public class AppConfiguration : PropertyChangedBase, ISettings
    {
        public SerializeConfigDTO ConfigDTO { get; set; }

        public AppConfiguration(SerializeConfigDTO configDTO)
        {
            ConfigDTO = configDTO;
        }

        /// <summary>
        /// Reads an appSetting. Read-only: the application <c>.config</c> sits in the install folder, which a standard user
        /// cannot write. The Wialon token is kept per user by <see cref="WialonTokenStore"/>.
        /// </summary>
        public object this[string propertyName]
        {
            get
            {
                return ConfigurationManager.AppSettings[propertyName];
            }
        }
        
    }
}
