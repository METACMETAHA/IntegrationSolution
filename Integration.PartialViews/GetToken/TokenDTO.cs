using IntegrationSolution.Common.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.PartialViews.GetToken
{
    public class TokenDTO : PropertyChangedBase
    {
        private string _token;
        public string Token
        {
            get => _token;
            set
            {
                _token = value;
                NotifyOfPropertyChange();
            }
        }

        private string _login;
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                NotifyOfPropertyChange();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
