using System;

namespace IntegrationSolution.Localization
{
    public sealed class LanguageChangedEventArgs : EventArgs
    {
        public LanguageChangedEventArgs(LanguageInfo oldLanguage, LanguageInfo newLanguage)
        {
            OldLanguage = oldLanguage;
            NewLanguage = newLanguage;
        }

        public LanguageInfo OldLanguage { get; }

        public LanguageInfo NewLanguage { get; }
    }
}
