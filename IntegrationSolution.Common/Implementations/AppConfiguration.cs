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
    public class AppConfiguration : PropertyChangedBase, ISettings, ICredentials
    {
        private BinaryFormatter formatter;
        private string binaryFileName;

        public AppConfiguration()
        {
            binaryFileName = "sys.dat";
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

        public CredentialsDTO GetCredentials()
        {
            formatter = new BinaryFormatter();
            CredentialsDTO result = null;

            try
            {
                using (FileStream fs = new FileStream(this.binaryFileName, FileMode.Open))
                    result = (CredentialsDTO)formatter.Deserialize(fs);
            }
            catch (Exception)
            { return new CredentialsDTO(); }
            return (result == null) ? new CredentialsDTO() : result;
        }

        public void SetCredentials(CredentialsDTO credentials)
        {
            formatter = new BinaryFormatter();

            using (FileStream fs = new FileStream(this.binaryFileName, FileMode.OpenOrCreate))
                formatter.Serialize(fs, credentials);
        }
    }
}
