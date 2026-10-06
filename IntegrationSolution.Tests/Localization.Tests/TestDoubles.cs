using IntegrationSolution.Localization.Resources;
using IntegrationSolution.Localization.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace IntegrationSolution.Tests.Localization.Tests
{
    internal sealed class InMemoryLanguagePreferenceStore : ILanguagePreferenceStore
    {
        public InMemoryLanguagePreferenceStore(string saved = null)
        {
            Saved = saved;
        }

        public string Saved { get; private set; }

        public List<string> SaveCalls { get; } = new List<string>();

        public Exception LoadException { get; set; }

        public Exception SaveException { get; set; }

        public string Load()
        {
            if (LoadException != null)
                throw LoadException;
            return Saved;
        }

        public void Save(string cultureName)
        {
            SaveCalls.Add(cultureName);
            if (SaveException != null)
                throw SaveException;
            Saved = cultureName;
        }
    }

    /// <summary>
    /// Resource manager over in-memory tables: culture name ("" = neutral) -> key -> value, with parent-culture fallback.
    /// </summary>
    internal sealed class DictionaryResourceManager : ResourceManager
    {
        private readonly Dictionary<string, Dictionary<string, string>> _tables;

        public DictionaryResourceManager(Dictionary<string, Dictionary<string, string>> tables)
        {
            _tables = tables;
        }

        public override string GetString(string name, CultureInfo culture)
        {
            for (var current = culture ?? CultureInfo.CurrentUICulture; ; current = current.Parent)
            {
                Dictionary<string, string> table;
                string value;
                var tableName = Equals(current, CultureInfo.InvariantCulture) ? string.Empty : current.Name;
                if (_tables.TryGetValue(tableName, out table) && table.TryGetValue(name, out value))
                    return value;
                if (Equals(current, CultureInfo.InvariantCulture))
                    return null;
            }
        }
    }

    /// <summary>
    /// The localization service changes process-wide culture state; restore it after every test.
    /// </summary>
    internal sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _uiCulture = Thread.CurrentThread.CurrentUICulture;
        private readonly CultureInfo _culture = Thread.CurrentThread.CurrentCulture;
        private readonly CultureInfo _defaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        private readonly CultureInfo _stringsCulture = Strings.Culture;

        public void Dispose()
        {
            Thread.CurrentThread.CurrentUICulture = _uiCulture;
            Thread.CurrentThread.CurrentCulture = _culture;
            CultureInfo.DefaultThreadCurrentUICulture = _defaultUiCulture;
            Strings.Culture = _stringsCulture;
        }
    }
}
