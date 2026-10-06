using IntegrationSolution.Localization.Resources;
using IntegrationSolution.Localization.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;

namespace IntegrationSolution.Localization
{
    /// <summary>
    /// Default <see cref="ILocalizationService"/> implementation backed by the
    /// <see cref="Strings"/> resource catalog (Strings.resx + satellite assemblies).
    /// </summary>
    /// <remarks>
    /// A single application-wide instance (<see cref="Instance"/>) is required because XAML markup extensions
    /// cannot receive dependencies from the DI container. The same instance is registered in the container
    /// as <see cref="ILocalizationService"/> for view models.
    /// </remarks>
    public sealed class LocalizationService : ILocalizationService
    {
        public const string DefaultCultureName = "en";

        /// <summary>
        /// Property name that refreshes every indexer binding (same value as System.Windows.Data.Binding.IndexerName).
        /// Kept as a literal so the service itself has no dependency on WPF types.
        /// </summary>
        public const string IndexerPropertyName = "Item[]";

        private static readonly Lazy<LocalizationService> _instance = new Lazy<LocalizationService>(
            () => new LocalizationService(Strings.ResourceManager, new UserSettingsLanguagePreferenceStore()),
            LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly object _sync = new object();
        private readonly ResourceManager _resourceManager;
        private readonly ILanguagePreferenceStore _preferenceStore;
        private LanguageInfo _currentLanguage;

        public LocalizationService(ResourceManager resourceManager, ILanguagePreferenceStore preferenceStore)
            : this(resourceManager, preferenceStore, CreateDefaultLanguages(), DefaultCultureName)
        { }

        public LocalizationService(
            ResourceManager resourceManager,
            ILanguagePreferenceStore preferenceStore,
            IEnumerable<LanguageInfo> supportedLanguages,
            string defaultCultureName)
        {
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            _preferenceStore = preferenceStore ?? throw new ArgumentNullException(nameof(preferenceStore));

            SupportedLanguages = (supportedLanguages ?? throw new ArgumentNullException(nameof(supportedLanguages)))
                .Distinct()
                .ToList()
                .AsReadOnly();

            if (SupportedLanguages.Count == 0)
                throw new ArgumentException("At least one language must be supported.", nameof(supportedLanguages));

            DefaultLanguage = FindSupported(defaultCultureName)
                ?? throw new ArgumentException($"Default culture '{defaultCultureName}' is not in the supported list.", nameof(defaultCultureName));

            _currentLanguage = DefaultLanguage;
        }

        /// <summary>
        /// Application-wide instance used by XAML (<c>{loc:Loc}</c>) and registered in the DI container.
        /// </summary>
        public static LocalizationService Instance => _instance.Value;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        public IReadOnlyList<LanguageInfo> SupportedLanguages { get; }

        public LanguageInfo DefaultLanguage { get; }

        public LanguageInfo CurrentLanguage
        {
            get { lock (_sync) return _currentLanguage; }
        }

        public CultureInfo CurrentCulture => CurrentLanguage.Culture;

        public string this[string key] => GetString(key);

        /// <summary>
        /// Languages shipped with the application. English is the neutral (fallback) language.
        /// </summary>
        public static IEnumerable<LanguageInfo> CreateDefaultLanguages()
        {
            yield return new LanguageInfo("en", "English");
            yield return new LanguageInfo("uk", "Українська");
            yield return new LanguageInfo("ru", "Русский");
        }

        /// <summary>
        /// Applies the persisted language (or the default one). Call once at startup, before any UI is created.
        /// </summary>
        public void Initialize()
        {
            string saved = null;
            try
            {
                saved = _preferenceStore.Load();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Localization: failed to load the saved UI language, using '{DefaultLanguage.CultureName}'. {ex}");
            }

            var language = FindSupported(saved);
            if (language == null && !string.IsNullOrWhiteSpace(saved))
                Trace.TraceWarning($"Localization: saved UI language '{saved}' is not supported, using '{DefaultLanguage.CultureName}'.");

            SetLanguage(language ?? DefaultLanguage, persist: false, force: true);
        }

        public bool ChangeLanguage(string cultureName)
        {
            var language = FindSupported(cultureName)
                ?? throw new ArgumentException($"UI language '{cultureName}' is not supported.", nameof(cultureName));

            return SetLanguage(language, persist: true, force: false);
        }

        public bool IsSupported(string cultureName) => FindSupported(cultureName) != null;

        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            string value = null;
            try
            {
                value = _resourceManager.GetString(key, CurrentCulture);
            }
            catch (MissingManifestResourceException ex)
            {
                Trace.TraceError($"Localization: resource catalog is missing. {ex}");
            }

            if (value == null)
            {
                Trace.TraceWarning($"Localization: missing resource key '{key}'.");
                return "[" + key + "]";
            }

            return value;
        }

        public string Format(string key, params object[] args)
        {
            var format = GetString(key);
            if (args == null || args.Length == 0)
                return format;

            try
            {
                // Values keep the regional formatting culture, exactly like the former hard-coded strings did.
                return string.Format(CultureInfo.CurrentCulture, format, args);
            }
            catch (FormatException ex)
            {
                Trace.TraceError($"Localization: invalid format string for key '{key}' ({CurrentCulture.Name}). {ex.Message}");
                return format;
            }
        }

        private bool SetLanguage(LanguageInfo language, bool persist, bool force)
        {
            LanguageInfo oldLanguage;
            lock (_sync)
            {
                oldLanguage = _currentLanguage;
                if (!force && oldLanguage.Equals(language))
                    return false;

                _currentLanguage = language;
                ApplyCulture(language.Culture);
            }

            if (persist)
            {
                try
                {
                    _preferenceStore.Save(language.CultureName);
                }
                catch (Exception ex)
                {
                    // The language is still switched for this session; only persistence failed.
                    Trace.TraceError($"Localization: failed to save the UI language '{language.CultureName}'. {ex}");
                }
            }

            OnPropertyChanged(IndexerPropertyName);
            OnPropertyChanged(nameof(CurrentLanguage));
            OnPropertyChanged(nameof(CurrentCulture));
            LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(oldLanguage, language));
            return true;
        }

        private static void ApplyCulture(CultureInfo culture)
        {
            // Strongly typed resources (Strings.SomeKey) resolve with this culture on every thread.
            Strings.Culture = culture;

            // UI language only: CurrentCulture (number/date formatting and parsing) stays as configured in Windows.
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        private LanguageInfo FindSupported(string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
                return null;

            cultureName = cultureName.Trim();

            var exact = SupportedLanguages.FirstOrDefault(
                x => string.Equals(x.CultureName, cultureName, StringComparison.OrdinalIgnoreCase));
            if (exact != null)
                return exact;

            // Accept specific cultures of a supported neutral language, e.g. "uk-UA" -> "uk".
            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(cultureName);
            }
            catch (CultureNotFoundException)
            {
                return null;
            }

            for (var current = culture; !Equals(current, CultureInfo.InvariantCulture); current = current.Parent)
            {
                var match = SupportedLanguages.FirstOrDefault(
                    x => string.Equals(x.CultureName, current.Name, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    return match;
            }

            return null;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
