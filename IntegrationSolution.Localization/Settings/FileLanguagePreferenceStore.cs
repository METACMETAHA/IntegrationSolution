using System;
using System.IO;
using System.Text;

namespace IntegrationSolution.Localization.Settings
{
    /// <summary>
    /// Stores the UI language as a one-line text file in the user's local application data folder
    /// (<c>%LocalAppData%\IntegrationSolution\ui-language.txt</c>), so no admin rights are needed.
    /// </summary>
    /// <remarks>
    /// Deliberately not a user-scoped <c>ApplicationSettingsBase</c> setting: on .NET Framework a corrupt
    /// user.config makes <c>ConfigurationManager</c> fail for the whole process (Wialon settings, log4net),
    /// while a corrupt preference file only makes the application fall back to the default language.
    /// </remarks>
    public sealed class FileLanguagePreferenceStore : ILanguagePreferenceStore
    {
        private readonly object _sync = new object();

        public FileLanguagePreferenceStore()
            : this(DefaultFilePath)
        { }

        public FileLanguagePreferenceStore(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A file path is required.", nameof(filePath));

            FilePath = filePath;
        }

        public static string DefaultFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "IntegrationSolution",
            "ui-language.txt");

        public string FilePath { get; }

        public string Load()
        {
            lock (_sync)
            {
                return File.Exists(FilePath)
                    ? File.ReadAllText(FilePath, Encoding.UTF8).Trim()
                    : null;
            }
        }

        public void Save(string cultureName)
        {
            if (cultureName == null)
                throw new ArgumentNullException(nameof(cultureName));

            lock (_sync)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                // Write a temporary file first, so an interrupted write never leaves a half-written preference.
                var temporaryPath = FilePath + ".tmp";
                File.WriteAllText(temporaryPath, cultureName, new UTF8Encoding(false));

                if (File.Exists(FilePath))
                    File.Replace(temporaryPath, FilePath, null);
                else
                    File.Move(temporaryPath, FilePath);
            }
        }
    }
}
