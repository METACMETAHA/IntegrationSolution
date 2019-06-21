using IntegrationSolution.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Interfaces
{
    public interface ICredentials
    {
        CredentialsDTO GetCredentials();

        void SetCredentials(CredentialsDTO credentials);
    }
}
