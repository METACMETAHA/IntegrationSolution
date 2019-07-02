using IntegrationSolution.Common.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Entities
{
    [Serializable]
    public class CredentialsDTO
    {
        private string login;
        public string Login
        {
            get { return login; }
            set
            { login = value; }
        }


        private string password;
        public string Password
        {
            get { return password; }
            set
            { password = value; }
        }
    }
}
