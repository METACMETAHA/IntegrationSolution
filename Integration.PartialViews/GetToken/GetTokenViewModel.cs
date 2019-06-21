using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Common.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.PartialViews.GetToken
{
    public class GetTokenViewModel : BindableBase
    {
        protected AppConfiguration _settings;


        #region Properties
        private TokenDTO _tokenModel;
        public TokenDTO TokenModel
        {
            get => _tokenModel;
            set { SetProperty(ref _tokenModel, value); }
        }

        private bool _isWithToken;
        public bool IsWithToken
        {
            get => _isWithToken;
            set { SetProperty(ref _isWithToken, value); }
        }
        #endregion


        public GetTokenViewModel(AppConfiguration settings)
        {
            _settings = settings;
            
            var credentials = settings.GetCredentials();
            TokenModel = new TokenDTO()
            {
                Token = settings["Token"]?.ToString(),
                Login = credentials.Login,
                Password = credentials.Password
            };

            if (!string.IsNullOrWhiteSpace(TokenModel.Token))
                IsWithToken = true;
            else
                IsWithToken = false;
        }
    }
}
