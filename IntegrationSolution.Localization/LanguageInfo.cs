using System;
using System.Globalization;

namespace IntegrationSolution.Localization
{
    /// <summary>
    /// Describes a UI language supported by the application.
    /// </summary>
    public sealed class LanguageInfo : IEquatable<LanguageInfo>
    {
        public LanguageInfo(string cultureName, string nativeName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
                throw new ArgumentException("Culture name is required.", nameof(cultureName));
            if (string.IsNullOrWhiteSpace(nativeName))
                throw new ArgumentException("Native name is required.", nameof(nativeName));

            Culture = CultureInfo.GetCultureInfo(cultureName);
            NativeName = nativeName;
        }

        /// <summary>
        /// Culture used to look up UI resources (e.g. "en", "uk", "ru").
        /// </summary>
        public CultureInfo Culture { get; }

        public string CultureName => Culture.Name;

        /// <summary>
        /// Name of the language written in that language ("English", "Українська", "Русский"),
        /// so a user can always find their language regardless of the current UI language.
        /// </summary>
        public string NativeName { get; }

        public bool Equals(LanguageInfo other)
        {
            return other != null && string.Equals(CultureName, other.CultureName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) => Equals(obj as LanguageInfo);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(CultureName);

        public override string ToString() => NativeName;
    }
}
