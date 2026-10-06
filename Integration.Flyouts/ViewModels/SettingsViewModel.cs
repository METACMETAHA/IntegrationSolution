using Integration.Flyouts.Implementations;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Common.Interfaces;
using IntegrationSolution.Localization;
using IntegrationSolution.Localization.Resources;
using NotificationConstructor.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Flyouts.ViewModels
{
    public class SettingsViewModel : FlyoutVMBase
    {
        private readonly ILocalizationService _localization;

        public SettingsViewModel(
            AppConfiguration settings,
            INotificationManager notificationManager,
            ILocalizationService localization)
            : base(
                  settings,
                  notificationManager)
        {
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
            _selectedLanguage = _localization.CurrentLanguage;

            this.Header = Strings.Settings_Header;
            LanguageChangedEventManager.AddHandler(_localization, OnLanguageChanged);
        }


        /// <summary>
        /// Languages offered in the settings, each shown by its native name.
        /// </summary>
        public IReadOnlyList<LanguageInfo> Languages => _localization.SupportedLanguages;


        private LanguageInfo _selectedLanguage;
        /// <summary>
        /// Selecting a language switches the UI immediately and saves the choice.
        /// </summary>
        public LanguageInfo SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (value == null || value.Equals(_selectedLanguage))
                    return;

                try
                {
                    // Raises LanguageChanged, which updates SelectedLanguage and the header.
                    if (_localization.ChangeLanguage(value.CultureName))
                        _notificationManager.NotifyInformationAsync(Strings.Settings_LanguageChanged);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Failed to switch the UI language to '{value.CultureName}'. {ex}");

                    // The exception may come from a listener after the switch itself succeeded.
                    var current = _localization.CurrentLanguage;
                    if (!current.Equals(value))
                        _notificationManager.NotifyErrorAsync(Strings.Settings_LanguageChangeFailed);

                    // Always re-sync the selector with the language that is actually active.
                    _selectedLanguage = current;
                    RaisePropertyChanged(nameof(SelectedLanguage));
                }
            }
        }


        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            this.Header = Strings.Settings_Header;
            SetProperty(ref _selectedLanguage, e.NewLanguage, nameof(SelectedLanguage));
        }
    }
}
