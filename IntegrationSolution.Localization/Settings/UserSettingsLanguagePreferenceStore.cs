namespace IntegrationSolution.Localization.Settings
{
    /// <summary>
    /// Stores the UI language in user-scoped .NET application settings.
    /// </summary>
    public sealed class UserSettingsLanguagePreferenceStore : ILanguagePreferenceStore
    {
        private readonly object _sync = new object();
        private LocalizationSettings _settings;

        public string Load()
        {
            lock (_sync)
            {
                var settings = GetSettings();
                if (settings.UpgradeRequired)
                {
                    // Carries the preference over from the previous application version, if any.
                    settings.Upgrade();
                    settings.UpgradeRequired = false;
                    settings.Save();
                }

                return settings.UiLanguage;
            }
        }

        public void Save(string cultureName)
        {
            lock (_sync)
            {
                var settings = GetSettings();
                settings.UiLanguage = cultureName;
                settings.UpgradeRequired = false;
                settings.Save();
            }
        }

        private LocalizationSettings GetSettings()
        {
            return _settings ?? (_settings = new LocalizationSettings());
        }
    }
}
