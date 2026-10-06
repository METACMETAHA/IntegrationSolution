using System.Configuration;

namespace IntegrationSolution.Localization.Settings
{
    /// <summary>
    /// User-scoped application settings (stored per Windows user in <c>%LocalAppData%\...\user.config</c>),
    /// so no write access to the installation folder is required.
    /// </summary>
    internal sealed class LocalizationSettings : ApplicationSettingsBase
    {
        [UserScopedSetting]
        [DefaultSettingValue(LocalizationService.DefaultCultureName)]
        [SettingsDescription("UI language (culture name) selected by the user.")]
        public string UiLanguage
        {
            get { return (string)this[nameof(UiLanguage)]; }
            set { this[nameof(UiLanguage)] = value; }
        }

        /// <summary>
        /// Standard flag used to migrate user settings after the application version changes.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool UpgradeRequired
        {
            get { return (bool)this[nameof(UpgradeRequired)]; }
            set { this[nameof(UpgradeRequired)] = value; }
        }
    }
}
