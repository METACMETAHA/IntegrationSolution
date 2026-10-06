using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace IntegrationSolution.Localization
{
    /// <summary>
    /// Provides localized UI strings and runtime switching of the UI language.
    /// </summary>
    /// <remarks>
    /// Only the UI language (<see cref="CultureInfo.CurrentUICulture"/>) is switched.
    /// Formatting/parsing culture (<see cref="CultureInfo.CurrentCulture"/>) is intentionally left untouched,
    /// so number/date handling of imported Excel data behaves exactly as before regardless of the selected language.
    /// </remarks>
    public interface ILocalizationService : INotifyPropertyChanged
    {
        /// <summary>
        /// Languages the user can choose from, in display order.
        /// </summary>
        IReadOnlyList<LanguageInfo> SupportedLanguages { get; }

        /// <summary>
        /// Language used when nothing (or something unsupported) is configured.
        /// </summary>
        LanguageInfo DefaultLanguage { get; }

        LanguageInfo CurrentLanguage { get; }

        CultureInfo CurrentCulture { get; }

        /// <summary>
        /// Localized string for <paramref name="key"/> in the current language.
        /// This indexer is the binding source used by the XAML markup extensions.
        /// </summary>
        string this[string key] { get; }

        string GetString(string key);

        /// <summary>
        /// Formats the localized string <paramref name="key"/> with <paramref name="args"/>.
        /// </summary>
        string Format(string key, params object[] args);

        /// <summary>
        /// Switches the UI language at runtime and persists the choice.
        /// </summary>
        /// <returns><c>true</c> when the language was changed; <c>false</c> when it was already active.</returns>
        /// <exception cref="ArgumentException">The culture is not supported.</exception>
        bool ChangeLanguage(string cultureName);

        bool IsSupported(string cultureName);

        /// <summary>
        /// Raised on the calling thread after the UI language was switched.
        /// Prefer <see cref="LanguageChangedEventManager"/> for long-lived subscriptions from short-lived objects.
        /// </summary>
        event EventHandler<LanguageChangedEventArgs> LanguageChanged;
    }
}
