using DialogConstruction.Interfaces;
using IntegrationSolution.Common.Enums;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Common.Interfaces;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NotificationConstructor.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications.Core;
using WialonBase.Interfaces;

namespace Integration.PartialViews.ViewModels
{
    public class GetTokenViewModel : BindableBase
    {
        protected readonly AppConfiguration _settings;
        protected readonly INavigationOperations _wialonContext;
        protected readonly IDialogManager _dialogManager;
        protected readonly INotificationManager _notificationManager;


        #region Properties
        private string _tokenModel;
        public string TokenModel
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


        public GetTokenViewModel(
            AppConfiguration settings,
            INavigationOperations navigationOperations,
            IDialogManager dialogManager,
            INotificationManager notificationManager)
        {
            _settings = settings;
            _dialogManager = dialogManager;
            _wialonContext = navigationOperations;
            _notificationManager = notificationManager;

            UpdateTokenCommand = new DelegateCommand(UpdateTokenCmd);

            TokenModel = settings["Token"]?.ToString();

            IsWithToken |= !string.IsNullOrWhiteSpace(TokenModel);

            //if (_wialonContext.TryConnect(settings["Token"]?.ToString()))
            //{
            //    TokenModel = settings["Token"]?.ToString();
            //    IsWithToken = true;
            //}
            //else
            //{
            //    settings["Token"] = string.Empty;
            //    IsWithToken = false;
            //}
        }


        #region Commands
        public DelegateCommand UpdateTokenCommand { get; private set; }
        protected async void UpdateTokenCmd()
        {
            if (IsWithToken)
            {
                var wnd = (MetroWindow)Application.Current.MainWindow;
                var progress = await wnd.ShowProgressAsync("Проверка токена", null);

                if (string.IsNullOrWhiteSpace(TokenModel))
                {
                    _notificationManager.NotifyErrorAsync("Токен пуст");
                    return;
                }

                bool tryConnect = false;
                await Task.Run(() =>
                {
                    tryConnect = _wialonContext.TryConnect(TokenModel);
                });
                if (tryConnect)
                {
                    _settings["Token"] = TokenModel;
                    _notificationManager.NotifySuccessAsync("Токен обновлен");
                }
                else
                {
                    TokenModel = _settings["Token"]?.ToString();
                    _notificationManager.NotifyWarningAsync("Восстановлен предыдущий токен");
                }
                if (progress.IsOpen)
                    await progress.CloseAsync();
            }
            else
            {
                await _dialogManager.ShowDialogAsync(DialogNamesEnum.InstructionsForToken);
                IsWithToken = true;
                _notificationManager.NotifyInformationAsync("Вставьте полученый токен в поле");
            }
            #endregion
        }
    }
}
