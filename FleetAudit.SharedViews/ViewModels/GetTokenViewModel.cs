using DialogConstruction.Interfaces;
using FleetAudit.Common.Enums;
using FleetAudit.Common.Implementations;
using FleetAudit.Common.Interfaces;
using FleetAudit.Localization.Resources;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NotificationConstructor.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications.Core;
using FleetAudit.Wialon.Interfaces;

namespace FleetAudit.SharedViews.ViewModels
{
    public class GetTokenViewModel : BindableBase
    {
        protected readonly WialonTokenStore _tokenStore;
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
            WialonTokenStore tokenStore,
            INavigationOperations navigationOperations,
            IDialogManager dialogManager,
            INotificationManager notificationManager)
        {
            _tokenStore = tokenStore;
            _dialogManager = dialogManager;
            _wialonContext = navigationOperations;
            _notificationManager = notificationManager;

            UpdateTokenCommand = new DelegateCommand(UpdateTokenCmd);

            TokenModel = _tokenStore.LoadOrLegacy();

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
                if (string.IsNullOrWhiteSpace(TokenModel))
                {
                    _notificationManager.NotifyErrorAsync(Strings.Token_Empty);
                    return;
                }

                var wnd = (MetroWindow)Application.Current.MainWindow;
                var progress = await wnd.ShowProgressAsync(Strings.Token_Checking, null);

                bool tryConnect = false;
                await Task.Run(() =>
                {
                    tryConnect = _wialonContext.TryConnect(TokenModel);
                });
                if (tryConnect && TrySaveToken(TokenModel))
                {
                    _wialonContext.UpdateToken();
                    _notificationManager.NotifySuccessAsync(Strings.Token_Updated);
                }
                else
                {
                    TokenModel = _tokenStore.LoadOrLegacy();
                    _notificationManager.NotifyWarningAsync(Strings.Token_PreviousRestored);
                }
                if (progress.IsOpen)
                    await progress.CloseAsync();
            }
            else
            {
                await _dialogManager.ShowDialogAsync(DialogNamesEnum.InstructionsForToken);
                IsWithToken = true;
                _notificationManager.NotifyInformationAsync(Strings.Token_PasteHint);
            }
            #endregion
        }

        private bool TrySaveToken(string token)
        {
            try
            {
                _tokenStore.Save(token);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to save the Wialon token. {ex}");
                _notificationManager.NotifyErrorAsync(Strings.Token_SaveFailed);
                return false;
            }
        }
    }
}
