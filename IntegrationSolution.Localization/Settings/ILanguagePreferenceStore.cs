namespace IntegrationSolution.Localization.Settings
{
    /// <summary>
    /// Persists the UI language chosen by the user.
    /// </summary>
    public interface ILanguagePreferenceStore
    {
        /// <summary>
        /// Returns the saved culture name, or <c>null</c>/empty when nothing was saved yet.
        /// </summary>
        string Load();

        void Save(string cultureName);
    }
}
